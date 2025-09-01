using AzureDayRoma2025.Agents;
using AzureDayRoma2025.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace AzureDayRoma2025.Orchestrations;

public static class TravelOrchestration
{

    [Function("TravelOrchestration_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
       [DurableClient] DurableTaskClient client,
       FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("TravelOrchestration_HttpStart");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
           nameof(TravelOrchestration), requestBody);


        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }

    [Function(nameof(TravelOrchestration))]
    public static async Task<TravelResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(TravelOrchestration));


        var input = context.GetInput<string>();

        // Replace name and input with values relevant for your Durable Functions Activity
        var model = await context.CallActivityAsync<TravelModel?>(nameof(TravelAgents.GetTravelModel), input);
        if (model is null)
        {
            logger.LogError("Travel model could not be extracted from input: {input}", input);
            return new TravelResult
            {
                ErrorMessage = "Travel model could not be extracted from input."
            };
        }

        var taskFlightsDeparture = context.CallActivityAsync<Flight[]>(nameof(TravelAgents.GetFlights), new FlightSearchModel
        {
            CityDeparture = model.CityDeparture,
            CityArrive = model.CityArrive,
            Date = model.DepartureDate
        });

        var taskFlightsArrival = context.CallActivityAsync<Flight[]>(nameof(TravelAgents.GetFlights), new FlightSearchModel
        {
            CityDeparture = model.CityArrive,
            CityArrive = model.CityDeparture,
            Date = model.ReturnDate
        });

        var taskActivities = context.CallActivityAsync<string?>(nameof(TravelAgents.GetActivities), model);

        await Task.WhenAll(taskFlightsDeparture, taskFlightsArrival, taskActivities);
        var flightsDeparture = taskFlightsDeparture.Result;
        var flightsArrival = taskFlightsArrival.Result;
        var activities = taskActivities.Result;

        return new TravelResult
        {
            FlightsDeparture = flightsDeparture,
            FlightsArrival = flightsArrival, // Assuming the same flights for arrival, adjust as needed
            Activities = activities,
            ErrorMessage = null
        };
    }




}
