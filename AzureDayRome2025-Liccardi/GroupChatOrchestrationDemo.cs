using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticKernelOrchestration;

public class GroupChatOrchestrationDemo
{
    private readonly Kernel _kernel;
    private readonly ChatCompletionAgent _writer;
    private readonly ChatCompletionAgent _editor;

    public GroupChatOrchestrationDemo(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

        _writer = new ChatCompletionAgent
        {
            Name = "CopyWriter",
            Instructions = "You are a copywriter with ten years of experience and are known for brevity and a dry humor. The goal is to refine and decide on the single best copy as an expert in the field. Only provide a single proposal per response. You're laser focused on the goal at hand. Don't waste time with chit chat. Consider suggestions when refining an idea.",
            Kernel = kernel,
            Description = "You are a copywriter with ten years of experience and are known for brevity and a dry humor. The goal is to refine and decide on the single best copy as an expert in the field. Only provide a single proposal per response. You're laser focused on the goal at hand. Don't waste time with chit chat. Consider suggestions when refining an idea."
        };

        _editor = new ChatCompletionAgent
        {
            Name = "Reviewer",
            Instructions = "You are an art director who has opinions about copywriting born of a love for David Ogilvy. The goal is to determine if the given copy is acceptable to print. If so, state that it is approved. If not, provide insight on how to refine suggested copy without example.",
            Kernel = kernel,
            Description = "You are an art director who has opinions about copywriting born of a love for David Ogilvy. The goal is to determine if the given copy is acceptable to print. If so, state that it is approved. If not, provide insight on how to refine suggested copy without example."
        };
    }

    public async Task<(string, List<ChatMessageContent>)> RunOrchestration(string prompt, InProcessRuntime runtime)
    {
        List<ChatMessageContent> history = [];

        ValueTask responseCallback(ChatMessageContent response)
        {
            history.Add(response);
            return ValueTask.CompletedTask;
        }

        #pragma warning disable SKEXP0110
        GroupChatOrchestration orchestration = new GroupChatOrchestration(
            new RoundRobinGroupChatManager { MaximumInvocationCount = 5 },
            _writer,
            _editor)
        {
            ResponseCallback = responseCallback,
        };

        var result = await orchestration.InvokeAsync(prompt, runtime);
        string output = await result.GetValueAsync(TimeSpan.FromSeconds(120));
        Console.WriteLine($"\n# RESULT: {output}");
        Console.WriteLine("\n\nORCHESTRATION HISTORY");
        foreach (ChatMessageContent message in history)
        {
            WriteAgentChatMessage(message);
        }
        #pragma warning restore SKEXP0110
        
        await runtime.RunUntilIdleAsync();
        return (output, history);
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
