using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AzureDayRoma2025.Agents;

public class AgentActivities(Kernel kernel)
{

    [Function("Agent1Activity")]
    public async Task<string> Agent1Activity([ActivityTrigger] string input, ILogger log)
    {
        var prompt = @"Riassumi il seguente testo:{{$input}}";
        var summaryFunction = kernel.CreateFunctionFromPrompt(prompt, functionName: "Summarizer");
        var result = await kernel.InvokeAsync(summaryFunction, new() { ["input"] = input });
        return result.ToString();
    }

    [Function("Agent2Activity")]
    public async Task<string> Agent2Activity([ActivityTrigger] string input, ILogger log)
    {
        var prompt = "Fornisci un'analisi dettagliata del seguente contenuto:\n{{$input}}";
        var analysisFunction = kernel.CreateFunctionFromPrompt(prompt, functionName: "Analyzer");
        var result = await kernel.InvokeAsync(analysisFunction, new() { ["input"] = input });
        return result.ToString();
    }

    [Function("Agent3Activity")]
    public async Task<string> Agent3Activity([ActivityTrigger] string input, ILogger log)
    {
        var prompt = "Scrivi una risposta ben formulata per questo contenuto:\n{{$input}}";
        var responseFunction = kernel.CreateFunctionFromPrompt(prompt, functionName: "Responder");
        var result = await kernel.InvokeAsync(responseFunction, new() { ["input"] = input });
        return result.ToString();
    }
}