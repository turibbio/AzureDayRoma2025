using AzureDayRoma2025.Agents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace AzureDayRoma2025.Orchestrations;

public static class OrchestrateChat
{
    [Function(nameof(OrchestrateChat))]
    public static async Task<IDictionary<string, string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<string>();

        var taskSummary = context.CallActivityAsync<string>(nameof(AgentActivities.Agent1Activity), input);
        var taskAnalysis = context.CallActivityAsync<string>(nameof(AgentActivities.Agent2Activity), input);
        var taskResponse = context.CallActivityAsync<string>(nameof(AgentActivities.Agent3Activity), input);

        await Task.WhenAll(taskSummary, taskAnalysis, taskResponse);

        return new Dictionary<string, string>
            {
                { "summary", taskSummary.Result },
                { "analysis", taskAnalysis.Result },
                { "response", taskResponse.Result }
            };
    }



    [Function("Orchestratechat_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("Orchestratechat_HttpStart");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(OrchestrateChat), requestBody);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}
