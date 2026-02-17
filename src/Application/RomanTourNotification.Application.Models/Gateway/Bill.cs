using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class Bill
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("r_id")]
    public int RequestId { get; init; }

    [JsonPropertyName("date")]
    public string Date { get; init; }

    [JsonPropertyName("number")]
    public string Number { get; init; }

    [JsonPropertyName("manager_id")]
    public int ManagerId { get; init; }

    [JsonPropertyName("price")]
    public string Price { get; set; }

    public decimal PriceDecimal { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; init; }

    public Bill(int id, int requestId, string date, string number, int managerId, string price, string createdAt)
    {
        Id = id;
        RequestId = requestId;
        Date = date;
        Number = number;
        ManagerId = managerId;
        Price = price;
        CreatedAt = createdAt;
    }

    public DateTime GetDate => DateTime.TryParse(Date, out DateTime date) ? date : DateTime.Today;
}