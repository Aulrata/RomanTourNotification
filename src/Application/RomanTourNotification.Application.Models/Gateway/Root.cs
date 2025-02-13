using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class Root
{
    [JsonPropertyName("requests")]
    public IEnumerable<Request>? Requests { get; init; }

    [JsonPropertyName("count_all")]
    public string? CountAll { get; init; }

    [JsonPropertyName("pages_all")]
    public int PagesAll { get; init; }
}