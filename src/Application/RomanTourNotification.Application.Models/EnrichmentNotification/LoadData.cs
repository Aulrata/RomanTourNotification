using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Models.EnrichmentNotification;

public class LoadData
{
    public string Name { get; set; } = string.Empty;

    public IEnumerable<Request>? Requests { get; set; } = new List<Request>();
}