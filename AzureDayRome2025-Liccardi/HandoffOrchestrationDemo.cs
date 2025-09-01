using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticKernelOrchestration;

public class HandoffOrchestrationDemo
{
    private readonly Kernel _kernel;
    private readonly ChatCompletionAgent triageAgent;
    private readonly ChatCompletionAgent statusAgent;
    private readonly ChatCompletionAgent returnAgent;
    private readonly ChatCompletionAgent refundAgent;

    // Plugin implementations
    public sealed class OrderStatusPlugin
    {
        [KernelFunction]
        public string CheckOrderStatus(string orderId) => $"Order {orderId} is shipped and will arrive in 2-3 days.";
    }
    public sealed class OrderReturnPlugin
    {
        [KernelFunction]
        public string ProcessReturn(string orderId, string reason) => $"Return for order {orderId} has been processed successfully.";
    }
    public sealed class OrderRefundPlugin
    {
        [KernelFunction]
        public string ProcessReturn(string orderId, string reason) => $"Refund for order {orderId} has been processed successfully.";
    }

    public HandoffOrchestrationDemo(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

        triageAgent = new ChatCompletionAgent
        {
            Name = "TriageAgent",
            Description = "Handle customer requests.",
            Instructions = "A customer support agent that triages issues.",
            Kernel = kernel,
        };

        statusAgent = new ChatCompletionAgent
        {
            Name = "OrderStatusAgent",
            Description = "A customer support agent that checks order status.",
            Instructions = "Handle order status requests.",
            Kernel = kernel,
        };
        statusAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderStatusPlugin()));

        returnAgent = new ChatCompletionAgent
        {
            Name = "OrderReturnAgent",
            Description = "A customer support agent that handles order returns.",
            Instructions = "Handle order return requests.",
            Kernel = kernel,
        };
        returnAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderReturnPlugin()));

        refundAgent = new ChatCompletionAgent
        {
            Name = "OrderRefundAgent",
            Description = "A customer support agent that handles order refund.",
            Instructions = "Handle order refund requests.",
            Kernel = kernel,
        };
        refundAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderRefundPlugin()));
    }

    public async Task<(string, List<ChatMessageContent>)> RunOrchestration(string prompt, InProcessRuntime runtime2)
    {
#pragma warning disable SKEXP0110
        var handoffs = OrchestrationHandoffs
            .StartWith(triageAgent)
            .Add(triageAgent, statusAgent, returnAgent, refundAgent)
            .Add(statusAgent, triageAgent, "Transfer to this agent if the issue is not status related")
            .Add(returnAgent, triageAgent, "Transfer to this agent if the issue is not return related")
            .Add(refundAgent, triageAgent, "Transfer to this agent if the issue is not refund related");

        List<ChatMessageContent> history = [];

        ValueTask responseCallback(ChatMessageContent response)
        {
            history.Add(response);
            return ValueTask.CompletedTask;
        }

        // Simulate user input with a queue
        Queue<string> responses = new();
        responses.Enqueue("I'd like to track the status of my order");
        responses.Enqueue("My order ID is 123");
        responses.Enqueue("I want to return another order of mine");
        responses.Enqueue("Order ID 321");
        responses.Enqueue("Broken item");
        responses.Enqueue("No, bye");

        ValueTask<ChatMessageContent> interactiveCallback()
        {
            string input = responses.Dequeue();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\n[User]: {input}");
            Console.ResetColor();
            return ValueTask.FromResult(new ChatMessageContent(AuthorRole.User, input));
        }

        HandoffOrchestration orchestration = new HandoffOrchestration(
            handoffs,
            triageAgent,
            statusAgent,
            returnAgent,
            refundAgent)
        {
            InteractiveCallback = interactiveCallback,
            ResponseCallback = responseCallback,
        };

        InProcessRuntime runtime = new InProcessRuntime();
        await runtime.StartAsync();

        string task = "I am a customer that needs help with my orders";
        var result = await orchestration.InvokeAsync(task, runtime);

        string output = await result.GetValueAsync(TimeSpan.FromSeconds(360));
        Console.WriteLine($"\n# RESULT: {output}");
        Console.WriteLine("\n\nORCHESTRATION HISTORY");
        foreach (ChatMessageContent message in history)
        {
            WriteAgentChatMessage(message);
        }

        await runtime.RunUntilIdleAsync();

        return (output, history);
#pragma warning restore SKEXP0110
    }

    private void WriteAgentChatMessage(ChatMessageContent message)
    {
        var originalColor = Console.ForegroundColor;

        if (message.Role == AuthorRole.Assistant)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            // Access metadata and get agent name if available
            string agentName = "Assistant";
            if (message.Metadata != null && message.Metadata.TryGetValue("agent", out var agent))
            {
                agentName = agent?.ToString() ?? "Assistant";
            }
            Console.WriteLine($"\n[{agentName}]: {message.Content}");
        }
        else if (message.Role == AuthorRole.User)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\n[User]: {message.Content}");
        }
        else if (message.Role == AuthorRole.System)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[System]: {message.Content}");
        }
        else
        {
            Console.WriteLine($"\n[{message.Role}]: {message.Content}");
        }

        Console.ForegroundColor = originalColor;
    }
}
