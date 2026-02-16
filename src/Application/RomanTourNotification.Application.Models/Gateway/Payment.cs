using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class Payment
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("date_create")]
    public string DateCreate { get; init; }

    [JsonPropertyName("reason")]
    public string Reason { get; init; }

    [JsonPropertyName("type_id")]
    public int PaymentTypeId { get; init; }

    public PaymentType PaymentType { get; init; }

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    public Payment(int id, string dateCreate, string reason, decimal price, int paymentTypeId)
    {
        Id = id;
        DateCreate = dateCreate;
        Reason = reason;
        Price = price;
        PaymentTypeId = paymentTypeId;
        PaymentType = (PaymentType)paymentTypeId;
    }
}