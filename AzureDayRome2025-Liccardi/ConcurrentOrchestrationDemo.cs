using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticKernelOrchestration;

public class ConcurrentOrchestrationDemo
{
    private readonly Kernel _kernel;
    private readonly ChatCompletionAgent _physicist;
    private readonly ChatCompletionAgent _chemist;

    public ConcurrentOrchestrationDemo(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

        // Initialize physicist agent
        _physicist = new ChatCompletionAgent
        {
            Name = "PhysicsExpert",
            Instructions = "You are an expert in physics. You answer questions from a physics perspective.",
            Kernel = _kernel,
            Description = "You are an expert in physics. You answer questions from a physics perspective. You are very concise and to the point."
        };

        // Initialize chemist agent
        _chemist = new ChatCompletionAgent
        {
            Name = "ChemistryExpert",
            Instructions = "You are an expert in chemistry. You answer questions from a chemistry perspective.",
            Kernel = _kernel,
            Description = "You are an expert in chemistry. You answer questions from a chemistry perspective. You are very concise and to the point."
        };
    }

    public async Task<string> RunOrchestration(string prompt, InProcessRuntime runtime)
    {
        #pragma warning disable SKEXP0110
        ConcurrentOrchestration orchestration = new(_physicist, _chemist);
        
        var result = await orchestration.InvokeAsync(prompt, runtime);
        
        string[] output = await result.GetValueAsync(TimeSpan.FromSeconds(360));
        #pragma warning restore SKEXP0110

        await runtime.RunUntilIdleAsync();

        // Format the result
        return $"# RESULTS FROM CONCURRENT EXPERTS:\n\n" +
               $"Physics Expert:\n{output[0]}\n\n" +
               $"Chemistry Expert:\n{output[1]}";
    }
}
