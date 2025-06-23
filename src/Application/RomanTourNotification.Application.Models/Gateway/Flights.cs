using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class Flights
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("date_begin")]
    public string DateBegin { get; init; }

    [JsonPropertyName("date_end")]
    public string DateEnd { get; init; }

    [JsonPropertyName("type_id")]
    public string? FlightsTypeId { get; init; }

    public FlightsType FlightsType { get; init; }

    public Flights(int id, string dateBegin, string dateEnd, string? flightsTypeId)
    {
        Id = id;
        DateBegin = dateBegin;
        DateEnd = dateEnd;
        FlightsTypeId = flightsTypeId;
        FlightsType = (FlightsType)int.Parse(flightsTypeId ?? "0");
    }

    public DateTime? DateBeginAsDate => DateTime.TryParse(DateBegin, out DateTime result) ? result : null;
}