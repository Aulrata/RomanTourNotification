using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class Employee
{
    [JsonPropertyName("global_u_id")]
    public long EmployeeId { get; init; }

    [JsonPropertyName("u_name")]
    public string FirstName { get; init; }

    [JsonPropertyName("u_surname")]
    public string Surname { get; init; }

    [JsonPropertyName("u_sname")]
    public string MiddleName { get; init; }

    [JsonPropertyName("role_id")]
    public int RoleId { get; init; }

    [JsonPropertyName("active")]
    public int Active { get; init; }

    public Employee(string firstName, string surname, string middleName)
    {
        FirstName = firstName;
        Surname = surname;
        MiddleName = middleName;
    }

    public string GetEmployeeFullName()
    {
        return $"{Surname} {FirstName} {MiddleName}";
    }
}