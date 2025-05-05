using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class RootRequests
{
    [JsonPropertyName("requests")]
    public IEnumerable<Request> Requests { get; init; }

    [JsonPropertyName("count_all")]
    public string CountAll { get; init; }

    [JsonPropertyName("pages_all")]
    public int PagesAll { get; init; }

    public RootRequests(
        IEnumerable<Request> requests,
        string countAll,
        int pagesAll)
    {
        Requests = requests;
        CountAll = countAll;
        PagesAll = pagesAll;
    }
}