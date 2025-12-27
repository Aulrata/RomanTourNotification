using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.ReturnNotification;
using RomanTourNotification.Application.Models.GoogleSheets;
using System.Text;

namespace RomanTourNotification.Application.ReturnNotification;

public class ReturnNotificationService : IReturnNotificationService
{
    private readonly ILoadSheetData _sheetsData;
    private readonly GoogleSheetsConfig _config;
    private readonly ILogger<ReturnNotificationService> _logger;
    private List<RowSheet> _rowSheets;
    private DateTime _loadData;

    public ReturnNotificationService(
        ILoadSheetData sheetsData,
        GoogleSheetsConfig config,
        ILogger<ReturnNotificationService> logger)
    {
        _sheetsData = sheetsData;
        _config = config;
        _logger = logger;
        _rowSheets = [];
    }

    public async Task<string> GetReturnMessageAsync(string manager, CancellationToken cancellationToken)
    {
        await LoadDataAsync(cancellationToken);

        string message;

        if (!string.IsNullOrEmpty(manager))
        {
            IEnumerable<RowSheet> filteredRows = _rowSheets
                .GroupBy(r => r.Manager)
                .First(x => x.Key.Split(' ').First().Equals(manager, StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation("Start crating return message for manager: {Manager}", manager);
            message = GetReturnMessage(filteredRows);
            _logger.LogInformation("Created return message for manager: {Manager}", manager);
        }
        else
        {
            _logger.LogInformation("Start crating return message for all managers");
            message = GetReturnMessage(_rowSheets);
            _logger.LogInformation("Start crating return message for all managers");
        }

        return string.IsNullOrEmpty(message) ? _config.TaskDescriptions?.NoTask ?? "Null" : message;
    }

    public async Task<string> GetReturnCompleteMessageAsync(CancellationToken cancellationToken)
    {
        await LoadDataAsync(cancellationToken);

        IEnumerable<RowSheet> completed = _rowSheets
            .Where(r => r is
            {
                SentStatementToTourist: true,
                GetStatementFromTourist: true,
                Completed: false,
            });

        string message = FormateMessage(completed, _config.TaskDescriptions?.CompletedTask);

        return string.IsNullOrEmpty(message) ? _config.TaskDescriptions?.NoTask ?? "Null" : message;
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        if (_loadData.Date != DateTime.Now.Date)
        {
            _rowSheets = (await _sheetsData.GetRowSheetsAsync(cancellationToken)).ToList();
            _loadData = DateTime.Now;
        }
    }

    private string GetReturnMessage(IEnumerable<RowSheet> rows)
    {
        var sb = new StringBuilder();

        var rowsList = rows.ToList();

        IEnumerable<RowSheet> sentStatementToTourist = rowsList
            .Where(r => r is
            {
                SentStatementToTourist: false,
                GetStatementFromTourist: false,
                Completed: false,
            });
        sb.Append(FormateMessage(sentStatementToTourist, _config.TaskDescriptions?.SentStatementToTouristTask));

        IEnumerable<RowSheet> getStatementFromTourist = rowsList
            .Where(r => r is
            {
                SentStatementToTourist: true,
                GetStatementFromTourist: false,
                Completed: false,
            });
        sb.Append(FormateMessage(getStatementFromTourist, _config.TaskDescriptions?.GetStatementFromTouristTask));

        return sb.ToString();
    }

    private string FormateMessage(IEnumerable<RowSheet> rows, string? task)
    {
        var rowGroup = rows.GroupBy(r => r.Manager).OrderBy(r => r.Key).ToList();

        if (rowGroup.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        sb.Append($"""
                   {_config.TaskDescriptions?.BaseTask}
                   <b><u>{task}</u></b>


                   """);

        foreach (IGrouping<string, RowSheet> rowList in rowGroup)
        {
            sb.Append($"""
                       <b>Менеджер: {rowList.Key}</b>


                       """);

            foreach (RowSheet row in rowList)
            {
                sb.Append($"""
                           {row.ToString()}


                           """);
            }
        }

        return sb.ToString();
    }
}