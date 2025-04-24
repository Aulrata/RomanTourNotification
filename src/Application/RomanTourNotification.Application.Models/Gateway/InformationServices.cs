using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class InformationServices
{
    [JsonPropertyName("id")]
    public int Id { get; private set; }

    [JsonPropertyName("request_id")]
    public int RequestId { get; private set; }

    [JsonPropertyName("date_begin")]
    public string DateBegin { get; private set; }

    [JsonPropertyName("date_end")]
    public string DateEnd { get; private set; }

    [JsonPropertyName("service_type_id")]
    public int ServiceTypeId { get; private set; }

    [JsonPropertyName("flights")]
    public IEnumerable<Flights>? Flights { get; init; }

    public InformationServiceType? InformationServiceType { get; init; }

    public InformationServices(
        int id,
        int requestId,
        string dateBegin,
        string dateEnd,
        int serviceTypeId)
    {
        Id = id;
        RequestId = requestId;
        DateBegin = dateBegin;
        DateEnd = dateEnd;
        ServiceTypeId = serviceTypeId;
        InformationServiceType = (InformationServiceType)serviceTypeId;
    }
}