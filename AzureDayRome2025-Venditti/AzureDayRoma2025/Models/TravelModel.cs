namespace AzureDayRoma2025.Models;

public class TravelModel
{
    public string? CityDeparture { get; set; }
    public string? CityArrive { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Days
    {
        get => (ReturnDate?.Date - DepartureDate?.Date)?.Days ?? 0;
    }
}