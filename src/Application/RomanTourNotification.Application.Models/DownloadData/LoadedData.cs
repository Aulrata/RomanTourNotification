using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Models.DownloadData;

public class LoadedData
{
    public string Name { get; set; } = string.Empty;

    public IEnumerable<Request>? Requests { get; set; } = new List<Request>();

    public string Url { get; set; } = string.Empty;
}