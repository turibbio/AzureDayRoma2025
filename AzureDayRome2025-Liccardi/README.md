# Semantic Kernel Orchestration Demo

This project demonstrates four types of orchestration with Semantic Kernel agents:

1. **Concurrent Orchestration** - Run multiple agents in parallel to get different perspectives on the same question
2. **Sequential Orchestration** - Pipeline multiple agents in sequence to complete a complex task
3. **Group Chat Orchestration** - Enable multiple agents to collaborate in a chat-like environment
4. **Handoff Orchestration** - Route requests to specialized agents based on criteria

## Prerequisites

- .NET 9.0 SDK
- Azure OpenAI API key and endpoint

## Setup

1. Copy the `.env.example` file to `.env`:
   ```
   cp .env.example .env
   ```

2. Edit the `.env` file and add your Azure OpenAI API key and endpoint.

## Running the Demo

Build and run the project:

```
dotnet build
dotnet run
```

You'll be presented with a menu where you can:

1. Run the Concurrent Orchestration demo (Physics & Chemistry Experts)
2. Run the Sequential Orchestration demo (Marketing Pipeline)
3. Run the Group Chat Orchestration demo (Copywriter & Reviewer)
4. Run the Handoff Orchestration demo (Customer Support)
5. Exit the application

## Project Structure

- `Program.cs` - Main application entry point and console UI
- `ConcurrentOrchestrationDemo.cs` - Demo of concurrent orchestration with Physics and Chemistry experts
- `SequentialOrchestrationDemo.cs` - Demo of sequential orchestration with Analyst, Writer, and Editor agents
- `GroupChatOrchestrationDemo.cs` - Demo of group chat orchestration with Writer and Editor agents
- `HandoffOrchestrationDemo.cs` - Demo of handoff orchestration with Triage, Status, Return, and Refund agents
- `GlobalUsings.cs` - Global using statements for the project

## Orchestration Patterns

### Concurrent Orchestration
In the concurrent orchestration pattern, multiple agents work simultaneously on the same query. Each agent brings its specific expertise to provide different perspectives on the same question. In this demo, a Physicist and a Chemist agent both answer questions from their respective viewpoints.

**Example Use Case:** Gathering multiple expert opinions on a complex question like "What is temperature?" to get both physics and chemistry perspectives.

### Sequential Orchestration
In the sequential orchestration pattern, multiple agents work in a pipeline, where each agent processes the output of the previous agent. This allows for a step-by-step processing of complex tasks. In this demo, an Analyst researches a topic, a Writer creates content based on the research, and an Editor polishes the final output.

**Example Use Case:** Creating marketing content by having one agent gather data, another draft content, and a third refine the final output.

### Group Chat Orchestration
In the group chat orchestration pattern, multiple agents collaborate in a conversation-like environment. Agents can respond to each other and build upon each other's ideas. In this demo, a Writer and an Editor collaborate to create and refine content.

**Example Use Case:** Collaborative content creation where agents can provide feedback and iterate on each other's work.

### Handoff Orchestration
In the handoff orchestration pattern, a primary agent evaluates queries and routes them to specialized agents based on specific criteria. In this demo, a Triage agent forwards customer support requests to specialized Status, Return, or Refund agents based on the nature of the request.

**Example Use Case:** Customer support system where initial queries are analyzed and routed to the most appropriate specialized agent.
