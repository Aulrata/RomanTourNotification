using RomanTourNotification.Application.Models.GoogleSheets;

namespace RomanTourNotification.Application.ReturnNotification.Cache;

/// <summary>
/// Singleton cache for Google Sheets return data.
/// Shared across DI scopes so the data survives scope disposal in the background service.
/// </summary>
public class ReturnDataCache
{
    private readonly object _lock = new();
    private List<RowSheet> _data = [];
    private DateTime _lastLoaded = DateTime.MinValue;

    /// <summary>Gets the cached rows.</summary>
    public IReadOnlyList<RowSheet> Data
    {
        get
        {
            lock (_lock)
                return _data;
        }
    }

    /// <summary>Gets the UTC timestamp of the last successful load.</summary>
    public DateTime LastLoaded
    {
        get
        {
            lock (_lock)
                return _lastLoaded;
        }
    }

    /// <summary>Replaces the cached data with <paramref name="rows"/> and records the load time.</summary>
    public void Update(IEnumerable<RowSheet> rows, DateTime utcNow)
    {
        lock (_lock)
        {
            _data = rows.ToList();
            _lastLoaded = utcNow;
        }
    }
}
