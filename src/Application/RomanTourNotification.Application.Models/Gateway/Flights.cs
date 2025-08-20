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
    public string FlightsTypeId { get; init; }

    public FlightsType FlightsType { get; init; }

    [JsonPropertyName("type")]
    public string FlightsTypeString { get; init; }

    public Flights(int id, string dateBegin, string dateEnd, string flightsTypeId, string flightsTypeString)
    {
        Id = id;
        DateBegin = dateBegin;
        DateEnd = dateEnd;
        FlightsTypeId = flightsTypeId;
        FlightsType = string.IsNullOrEmpty(flightsTypeString) ? (FlightsType)int.Parse(flightsTypeId ?? "0") : MapperFlightsType(flightsTypeString);
        FlightsTypeString = flightsTypeString;
    }

    public DateTime? DateBeginAsDate => DateTime.TryParse(DateBegin, out DateTime result) ? result : null;

    public DateTime? DateEndAsDate => DateTime.TryParse(DateEnd, out DateTime result) ? result : null;

    private FlightsType MapperFlightsType(string flightsType)
    {
        return flightsType.ToLower() switch
        {
            "блок мест" => FlightsType.BlockOfSeats,
            _ => FlightsType.Unspecified,
        };
    }
}