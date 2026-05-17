using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class RootRequest
{
    [JsonPropertyName("request")]
    public IEnumerable<Request> Request { get; init; }

    [JsonPropertyName("extended_fields")]
    public IEnumerable<string> ExtendedFields { get; init; }

    [JsonPropertyName("is_archive")]
    public int IsArchive { get; init; }

    public RootRequest(
        IEnumerable<Request> request,
        IEnumerable<string> extendedFields,
        int isArchive)
    {
        Request = request;
        ExtendedFields = extendedFields;
        IsArchive = isArchive;
    }
}