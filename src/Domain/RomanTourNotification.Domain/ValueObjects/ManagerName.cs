namespace RomanTourNotification.Domain.ValueObjects;

/// <summary>
/// Represents a manager's full name stored as "Surname Name Patronymic".
/// <see cref="LastName"/> safely returns the first word,
/// falling back to <see cref="Full"/> when the string is a single token.
/// </summary>
public record ManagerName
{
    /// <summary>Gets the full name string.</summary>
    public string Full { get; }

    /// <summary>Gets the first word of the full name (typically the surname).</summary>
    public string LastName { get; }

    /// <summary>Initializes a new instance of the <see cref="ManagerName"/> class.</summary>
    public ManagerName(string full)
    {
        Full = full ?? string.Empty;
        string[] parts = Full.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        LastName = parts.Length > 0 ? parts[0] : Full;
    }
}
