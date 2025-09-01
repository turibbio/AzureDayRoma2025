using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticKernelOrchestration;

public class SequentialOrchestrationDemo
{
    private readonly Kernel _kernel;
    private readonly ChatCompletionAgent _analystAgent;
    private readonly ChatCompletionAgent _writerAgent;
    private readonly ChatCompletionAgent _editorAgent;

    public SequentialOrchestrationDemo(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

        // Define analyst agent for marketing task
        _analystAgent = new ChatCompletionAgent
        {
            Name = "Analyst",
            Instructions = "You are a marketing analyst. Given a product description, identify:\n- Key features\n- Target audience\n- Unique selling points",
            Kernel = _kernel,
            Description = "You are a marketing analyst. Given a product description, identify key features, target audience, and unique selling points. Output should be structured as JSON with keys: 'features', 'audience', 'usps'."
        };

        // Define copywriter agent to create marketing copy
        _writerAgent = new ChatCompletionAgent
        {
            Name = "Copywriter",
            Instructions = "You are a marketing copywriter. Given a block of text describing features, audience, and USPs, compose a compelling marketing copy (like a newsletter section) that highlights these points. Output should be short (around 150 words), output just the copy as a single text block.",
            Kernel = _kernel,
            Description = "You are a marketing copywriter. Given a block of text describing features, audience, and USPs, compose a compelling marketing copy (like a newsletter section) that highlights these points. Output should be short (around 150 words), output just the copy as a single text block."
        };

        // Define editor agent to polish the copy
        _editorAgent = new ChatCompletionAgent
        {
            Name = "Editor",
            Instructions = "You are an editor. Given the draft copy, correct grammar, improve clarity, ensure consistent tone, give format and make it polished. Output the final improved copy as a single text block.",
            Kernel = _kernel,
            Description = "You are an editor. Given the draft copy, correct grammar, improve clarity, ensure consistent tone, give format and make it polished. Output the final improved copy as a single text block."
        };
    }

    public async Task<(string result, List<ChatMessageContent> history)> RunOrchestration(string prompt, InProcessRuntime runtime)
    {
        List<ChatMessageContent> history = new();

        ValueTask ResponseCallback(ChatMessageContent response)
        {
            history.Add(response);
            return ValueTask.CompletedTask;
        }

        // Create sequential orchestration with the three agents
        #pragma warning disable SKEXP0110
        SequentialOrchestration orchestration = new(_analystAgent, _writerAgent, _editorAgent)
        {
            ResponseCallback = ResponseCallback,
        };

        // Run the orchestration with the given prompt
        var result = await orchestration.InvokeAsync(prompt, runtime);

        // Get the result
        string output = await result.GetValueAsync(TimeSpan.FromSeconds(360));
        #pragma warning restore SKEXP0110
        
        await runtime.RunUntilIdleAsync();

        return (output, history);
    }
}
