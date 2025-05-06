using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.DownloadData;

public class LoadEmployees : ILoadEmployees
{
    private readonly IGatewayService _gatewayService;
    private readonly IEnumerable<ApiSettings> _apiSettings;
    private readonly ILogger<LoadEmployees> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly List<Employee> _employees;
    private DateTime _updateDate;

    public LoadEmployees(
        IGatewayService gatewayService,
        IEnumerable<ApiSettings> apiSettings,
        ILogger<LoadEmployees> logger)
    {
        _gatewayService = gatewayService;
        _apiSettings = apiSettings;
        _logger = logger;
        _employees = [];
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken)
    {
        if (_updateDate != DateTime.Today.Date)
            await LoadAllEmployeesAsync(cancellationToken);

        IEnumerable<Employee> result = _employees
            .Where(e => e is { Active: 1, RoleId: 1 or 2 })
            .DistinctBy(e => e.GetEmployeeFullName());

        return result;
    }

    private async Task LoadAllEmployeesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Employee loading has begun");
        _employees.Clear();
        foreach (ApiSettings apiSetting in _apiSettings)
        {
            ContextDto context = await _gatewayService.GetAllEmployeeAsync(apiSetting.Api, cancellationToken);

            RootEmployee? result = JsonSerializer.Deserialize<RootEmployee>(context.Stream, _jsonSerializerOptions);

            if (result is null) continue;

            if (!result.EmployeesList.Any())
            {
                _logger.LogInformation("No employees found");
                continue;
            }

            _employees.AddRange(result.EmployeesList);
        }

        _updateDate = DateTime.Today.Date;
    }
}