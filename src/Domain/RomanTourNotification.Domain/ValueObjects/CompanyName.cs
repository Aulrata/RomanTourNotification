namespace RomanTourNotification.Domain.ValueObjects;

/// <summary>
/// Represents a company name of the form "ИП Surname ...".
/// <see cref="ShortName"/> safely returns the second word (e.g. the surname),
/// falling back to <see cref="Full"/> when the name has only one part.
/// </summary>
public record CompanyName
{
    /// <summary>Gets the full company name string.</summary>
    public string Full { get; }

    /// <summary>Gets the second word of the company name (typically the owner surname).</summary>
    public string ShortName { get; }

    /// <summary>Initializes a new instance of the <see cref="CompanyName"/> class.</summary>
    public CompanyName(string full)
    {
        Full = full ?? string.Empty;
        string[] parts = Full.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        ShortName = parts.Length > 1 ? parts[1] : Full;
    }
}
