using AzureDayRoma2025.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace AzureDayRoma2025.Agents;

public class TravelAgents(Kernel kernel)
{
    [Function("GetTravelModel")]
    public async Task<TravelModel?> GetTravelModel([ActivityTrigger] string input, ILogger log)
    {
        var prompt = @"
system: sei un interprete esperto di testo, specializzato nell'estrazione di informazioni da testi complessi.
input: 
{{$input}}: un testo che contiene informazioni su un viaggio, come città di partenza, città di arrivo, data di partenza e data di ritorno.
output:  solo json deserializzabile schema:
{
 ""CityDeparture"":""città di partenza"",
    ""CityArrive"":""città di arrivo"",
 ""DepartureDate"":""data partenza"", 
""ReturnDate"":""data ritorno""
}

Estrai le informazioni di città di partenza, città di arrivo, data partenza e data arriva da {{$input}}.
il risultato deve essere un oggetto JSON con le seguenti proprietà:
CityDeparture, CityArrive, DepartureDate, ReturnDate.
le proprietà ReturnDate e DepartureDate sono di tipo datetime
";

        var function = kernel.CreateFunctionFromPrompt(prompt, functionName: "TravelModelCreator");

        var resultstr = await kernel.InvokeAsync<string>(function, new() { ["input"] = input });
        resultstr = resultstr?.Replace("```json", "").Replace("```", "").Trim();
        if (string.IsNullOrWhiteSpace(resultstr))
        {
            log.LogError("No valid JSON result returned from the function. Input: {input}", input);
            return null;
        }

        var result = JsonSerializer.Deserialize<TravelModel>(resultstr, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return result;
    }

    [Function("GetFlights")]
    public Task<Flight[]> GetFlights([ActivityTrigger] FlightSearchModel? model, ILogger log)
    {
        if (model is null)
            return Task.FromResult(Array.Empty<Flight>());

        var results = Flights
            .Where(flight => flight.From.Equals(model.CityDeparture, StringComparison.OrdinalIgnoreCase) &&
                            flight.To.Equals(model.CityArrive, StringComparison.OrdinalIgnoreCase) &&
                            flight.Departure.Date == model.Date?.Date)
            .ToArray();

        return Task.FromResult(results);
    }

    [Function("GetActivities")]
    public async Task<string?> GetActivities([ActivityTrigger] TravelModel? model, ILogger log)
    {
        var prompt = @$"
System: Sei un agente di viaggio esperto, specializzato nella creazione di itinerari turistici personalizzati
input: 
{model.CityArrive}: la città da visitare
{model.Days}: il numero di giorni di permanenza
output:
Crea un programma di attività turistiche,
per un adulto per circa 8 ore al giorno, 
focalizzati sul attività culturali e gastronomiche.
produci un elenco di attività dettagliate con riferimenti ai luoghi da visitare con indicazioni del giorno e orario previsti.
";


        var days = (model?.ReturnDate?.Date - model?.DepartureDate?.Date)?.Days ?? 0;

        var input = new { CityDeparture = model.CityArrive, Days = days };

        var function = kernel.CreateFunctionFromPrompt(prompt, functionName: "GetActivitiesCreator");
        var result = await kernel.InvokeAsync<string>(function);
        return result;
    }

    private static IReadOnlyCollection<Flight> Flights = new List<Flight>
    {
        new Flight { From = "Milano", To = "Roma", Departure = new DateTime(2025, 6, 20, 10, 0, 0) },
        new Flight { From = "Roma", To = "Milano", Departure = new DateTime(2025, 6, 27, 15, 0, 0) },
        new Flight { From = "Roma", To = "Milano", Departure = new DateTime(2025, 6, 22, 15, 0, 0) }
    };
}
