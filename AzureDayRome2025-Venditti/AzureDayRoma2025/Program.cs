using AzureDayRoma2025.Agents;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddScoped<Kernel>(provider =>
{
    var builder = Kernel.CreateBuilder();

    var deploymentName = Environment.GetEnvironmentVariable("Agent_deploymentName");
    var apiKey = Environment.GetEnvironmentVariable("Agent_apiKey");
    var endpoint = Environment.GetEnvironmentVariable("Agent_endpoint");

    builder.AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        apiKey: apiKey,
        endpoint: endpoint);
    return builder.Build();
});

builder.Services.AddScoped<AgentActivities>();
builder.Services.AddScoped<TravelAgents>();

builder.Build().Run();
