# Durable Function Agent Orchestration 

This demo project contains two durable functions with orchestration example

1. **OrchestrateChat**
Parallel Agents Orchestration
2. **TravelOrchestration**
Orchestration Agents as a Business Flow

## Prerequisites

- .NET 9.0 SDK
- Azure OpenAI API key and endpoint
- Docker


## Setup

1. Install [Durable Task Scheduler Dashboard](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-task-scheduler/quickstart-durable-task-scheduler?pivots=csharp#set-up-the-durable-task-emulator)

Locally you can launch a docker container


```
docker run -d -p 8080:8080 -p 8082:8082 mcr.microsoft.com/dts/dts-emulator:latest
```

2. Create local.settings.json
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DURABLE_TASK_SCHEDULER_CONNECTION_STRING": "Endpoint=http://localhost:8080;Authentication=None",
    "TASKHUB_NAME": "default",
    "Agent_deploymentName": "you agent name",
    "Agent_apiKey": "your apy key",
    "Agent_endpoint": "your agente endpoint"
  }
}
```