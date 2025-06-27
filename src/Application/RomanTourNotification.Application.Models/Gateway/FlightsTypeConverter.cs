using RomanTourNotification.Application.Models.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class FlightsTypeConverter : JsonConverter<FlightsType>
{
    public override FlightsType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return FlightsType.Unspecified;
        }

        return value.ToLower() switch
        {
            "блок мест" => FlightsType.BlockOfSeats,
            _ => FlightsType.Unspecified,
        };
    }

    public override void Write(Utf8JsonWriter writer, FlightsType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            FlightsType.BlockOfSeats => FlightsType.BlockOfSeats.GetDescription(),
            FlightsType.Unspecified => FlightsType.Unspecified.GetDescription(),
            FlightsType.Regular => FlightsType.Regular.GetDescription(),
            FlightsType.Charter => FlightsType.Charter.GetDescription(),
            _ => string.Empty,
        });
    }
}