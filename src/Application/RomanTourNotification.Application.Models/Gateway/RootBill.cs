using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class RootBill
{
    [JsonPropertyName("records")]
    public IEnumerable<Bill> Bills { get; set; }

    public RootBill(IEnumerable<Bill> bills)
    {
        Bills = bills;
    }
}