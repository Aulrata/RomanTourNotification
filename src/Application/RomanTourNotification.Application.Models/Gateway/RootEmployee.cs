using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.Models.Gateway;

public class RootEmployee
{
    [JsonPropertyName("users")]
    public IEnumerable<Employee> EmployeesList { get; init; }

    public RootEmployee(IEnumerable<Employee> employeesList)
    {
        EmployeesList = employeesList;
    }
}