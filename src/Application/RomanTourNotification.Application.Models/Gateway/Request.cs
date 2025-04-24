using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class Request
{
    [JsonPropertyName("id")]
    public int Id { get; private set; }

    [JsonPropertyName("id_system")]
    public int IdSystem { get; private set; }

    [JsonPropertyName("date_begin")]
    public string DateBegin { get; private set; }

    [JsonPropertyName("date_end")]
    public string DateEnd { get; private set; }

    [JsonPropertyName("supplier_name")]
    public string SupplierName { get; private set; }

    [JsonPropertyName("client_surname")]
    public string ClientLastName { get; private set; }

    [JsonPropertyName("client_name")]
    public string ClientFirstName { get; private set; }

    [JsonPropertyName("client_sname")]
    public string ClientMiddleName { get; private set; }

    [JsonPropertyName("client_email")]
    public string ClientEmail { get; private set; }

    [JsonPropertyName("status_id")]
    public string StatusId { get; private set; }

    [JsonPropertyName("services")]
    public IEnumerable<InformationServices> Services { get; init; }

    public Request(
        int id,
        int idSystem,
        string dateBegin,
        string supplierName,
        string clientLastName,
        string clientFirstName,
        string clientMiddleName,
        string clientEmail,
        string dateEnd,
        string statusId,
        IEnumerable<InformationServices> services)
    {
        Id = id;
        IdSystem = idSystem;
        DateBegin = dateBegin;
        SupplierName = supplierName;
        ClientLastName = clientLastName;
        ClientFirstName = clientFirstName;
        ClientMiddleName = clientMiddleName;
        ClientEmail = clientEmail;
        DateEnd = dateEnd;
        StatusId = statusId;
        Services = services;
    }
}