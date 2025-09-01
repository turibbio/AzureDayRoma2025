namespace AzureDayRoma2025.Models;

public class TravelResult
{
    public Flight[] FlightsDeparture { get; set; }
    public Flight[] FlightsArrival { get; set; }
    public string? Activities { get; set; }
    public string? ErrorMessage { get; set; }
}
