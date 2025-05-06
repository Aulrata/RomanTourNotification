using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class Request
{
    [JsonPropertyName("id")]
    public int Id { get; private set; }

    [JsonPropertyName("id_system")]
    public int IdSystem { get; private set; }

    [JsonPropertyName("dat_request")]
    public string DateRequest { get; private set; }

    [JsonPropertyName("date_begin")]
    public string DateBegin { get; private set; }

    [JsonPropertyName("date_end")]
    public string DateEnd { get; private set; }

    [JsonPropertyName("supplier_name")]
    public string SupplierName { get; private set; }

    [JsonPropertyName("client_surname")]
    public string ClientSurname { get; private set; }

    [JsonPropertyName("client_name")]
    public string ClientFirstName { get; private set; }

    [JsonPropertyName("client_sname")]
    public string ClientMiddleName { get; private set; }

    [JsonPropertyName("client_email")]
    public string ClientEmail { get; private set; }

    [JsonPropertyName("status_id")]
    public string StatusId { get; private set; }

    public RequestStatus Status { get; private set; }

    [JsonPropertyName("services")]
    public IEnumerable<InformationServices> Services { get; init; }

    [JsonPropertyName("calc_price")]
    public double? CalcPrice { get; set; }

    [JsonPropertyName("calc_client")]
    public double? CalcClient { get; set; }

    [JsonPropertyName("manager_surname")]
    public string ManagerSurname { get; set; }

    [JsonPropertyName("manager_name")]
    public string ManagerName { get; set; }

    [JsonPropertyName("manager_sname")]
    public string ManagerMiddleName { get; set; }

    [JsonPropertyName("payment_deadline_client")]
    public string PaymentDeadlineClient { get; set; }

    [JsonPropertyName("company_name_rus")]
    public string CompanyNameRus { get; set; }

    public Request(
        int id,
        int idSystem,
        string dateRequest,
        string dateBegin,
        string supplierName,
        string clientSurname,
        string clientFirstName,
        string clientMiddleName,
        string clientEmail,
        string dateEnd,
        string statusId,
        IEnumerable<InformationServices> services,
        string managerSurname,
        string managerName,
        string managerMiddleName,
        string paymentDeadlineClient,
        string companyNameRus)
    {
        Id = id;
        IdSystem = idSystem;
        DateBegin = dateBegin;
        SupplierName = supplierName;
        ClientSurname = clientSurname;
        ClientFirstName = clientFirstName;
        ClientMiddleName = clientMiddleName;
        ClientEmail = clientEmail;
        DateEnd = dateEnd;
        StatusId = statusId;
        Services = services;
        ManagerSurname = managerSurname;
        ManagerName = managerName;
        ManagerMiddleName = managerMiddleName;
        PaymentDeadlineClient = paymentDeadlineClient;
        CompanyNameRus = companyNameRus;
        DateRequest = dateRequest;
        Status = (RequestStatus)int.Parse(StatusId);
    }

    public DateTime? DateBeginAsDate => DateTime.
        TryParse(DateBegin, out DateTime result) ? result.Date : null;

    public DateTime? DateRequestAsDate => DateTime.
        TryParse(DateRequest, out DateTime result) ? result.Date : null;

    public DateTime? DatePaymentDeadline => DateTime.
        TryParse(PaymentDeadlineClient, out DateTime result) ? result.Date : null;

    public double ClientDebt => (CalcPrice ?? 0.00) - (CalcClient ?? 0.00);

    public string ManagerFullName => $"{ManagerSurname} {ManagerName} {ManagerMiddleName}";

    public string CompanyNameShort => CompanyNameRus.Split(' ')[1];
}