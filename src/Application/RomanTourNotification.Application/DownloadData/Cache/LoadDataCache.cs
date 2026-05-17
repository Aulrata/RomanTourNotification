using RomanTourNotification.Application.Models.DownloadData;

namespace RomanTourNotification.Application.DownloadData.Cache;

/// <summary>
/// Thread-safe singleton that holds the in-memory cache of loaded CRM requests.
/// Keeping the state here (instead of inside the Scoped LoadDataService) means
/// the cache survives across DI scopes.
/// </summary>
public class LoadDataCache
{
    private readonly object _lock = new();
    private List<LoadedData> _data = [];
    private DateTime _lastLoaded = DateTime.MinValue;

    /// <summary>Gets the timestamp of the last successful load.</summary>
    public DateTime LastLoaded
    {
        get { lock (_lock) return _lastLoaded; }
    }

    /// <summary>Gets a snapshot of the currently cached data.</summary>
    public IReadOnlyList<LoadedData> Data
    {
        get { lock (_lock) return _data.AsReadOnly(); }
    }

    /// <summary>Atomically replaces the cache content and updates the timestamp.</summary>
    public void Update(IEnumerable<LoadedData> data, DateTime loadedAt)
    {
        lock (_lock)
        {
            _data = data.ToList();
            _lastLoaded = loadedAt;
        }
    }
}
