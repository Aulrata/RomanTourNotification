using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Abstractions.Time;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.ReturnNotification;
using RomanTourNotification.Application.Models.GoogleSheets;
using RomanTourNotification.Application.ReturnNotification.Cache;
using RomanTourNotification.Domain.ReturnWorkflow;
using RomanTourNotification.Domain.ValueObjects;
using System.Text;

namespace RomanTourNotification.Application.ReturnNotification;

public class ReturnNotificationService : IReturnNotificationService
{
    private readonly ILoadSheetData _sheetsData;
    private readonly GoogleSheetsConfig _config;
    private readonly ILogger<ReturnNotificationService> _logger;
    private readonly ReturnDataCache _cache;
    private readonly IClock _clock;

    /// <summary>Initializes a new instance of the <see cref="ReturnNotificationService"/> class.</summary>
    public ReturnNotificationService(
        ILoadSheetData sheetsData,
        GoogleSheetsConfig config,
        ILogger<ReturnNotificationService> logger,
        ReturnDataCache cache,
        IClock clock)
    {
        _sheetsData = sheetsData;
        _config = config;
        _logger = logger;
        _cache = cache;
        _clock = clock;
    }

    /// <inheritdoc/>
    public async Task<string> GetReturnMessageAsync(string manager, CancellationToken cancellationToken)
    {
        await RefreshCacheAsync(cancellationToken);

        string message;

        if (!string.IsNullOrEmpty(manager))
        {
            IGrouping<string, RowSheet>? managerGroup = _cache.Data
                .GroupBy(r => r.Manager)
                .FirstOrDefault(x => new ManagerName(x.Key).LastName
                    .Equals(manager, StringComparison.OrdinalIgnoreCase));

            if (managerGroup is null)
            {
                _logger.LogWarning("No return rows found for manager: {Manager}", manager);
                return _config.TaskDescriptions?.NoTask ?? "Null";
            }

            _logger.LogInformation("Creating return message for manager: {Manager}", manager);
            message = BuildReturnMessage(managerGroup);
            _logger.LogInformation("Created return message for manager: {Manager}", manager);
        }
        else
        {
            _logger.LogInformation("Creating return message for all managers");
            message = BuildReturnMessage(_cache.Data);
            _logger.LogInformation("Created return message for all managers");
        }

        return string.IsNullOrEmpty(message) ? _config.TaskDescriptions?.NoTask ?? "Null" : message;
    }

    /// <inheritdoc/>
    public async Task<string> GetReturnCompleteMessageAsync(CancellationToken cancellationToken)
    {
        await RefreshCacheAsync(cancellationToken);

        IEnumerable<RowSheet> pendingReceipt = _cache.Data
            .Where(r => !string.IsNullOrEmpty(r.Sum) && r.Status == ReturnStatus.PendingReceipt);

        string message = FormatMessage(pendingReceipt, _config.TaskDescriptions?.ReceiptPrintedTask);

        return string.IsNullOrEmpty(message) ? _config.TaskDescriptions?.NoTask ?? "Null" : message;
    }

    private async Task RefreshCacheAsync(CancellationToken cancellationToken)
    {
        if (_cache.LastLoaded.Date == _clock.UtcNow.Date)
            return;

        IEnumerable<RowSheet> rows = (await _sheetsData.GetRowSheetsAsync(cancellationToken))
            .Where(r => string.IsNullOrEmpty(r.ReturnedSum));

        _cache.Update(rows, _clock.UtcNow);
    }

    private string BuildReturnMessage(IEnumerable<RowSheet> rows)
    {
        // Each entry is one section in the message.
        // To add a new section: add one line here + one property in TaskDescriptions + appsettings.json.
        //
        // Branch A — Sum is empty:
        //   SentApplicationToTourOperator = false → send refund application to tour operator
        //
        // Branch B — Sum is not empty (strict linear pipeline, Completed rows are excluded implicitly):
        //   PendingSend       → send statement to tourist
        //   PendingReceive    → receive statement from tourist
        //   PendingAccounting → send statement to accounting
        //   PendingReceipt    → print receipt
        (Func<RowSheet, bool> Filter, string? Title)[] sections =
        [
            (r => string.IsNullOrEmpty(r.Sum) && !r.GetSentApplicationToTourOperator,
                _config.TaskDescriptions?.SentApplicationToTourOperatorTask),

            (r => !string.IsNullOrEmpty(r.Sum) && r.Status == ReturnStatus.PendingSend,
                _config.TaskDescriptions?.SentStatementToTouristTask),

            (r => !string.IsNullOrEmpty(r.Sum) && r.Status == ReturnStatus.PendingReceive,
                _config.TaskDescriptions?.GetStatementFromTouristTask),

            (r => !string.IsNullOrEmpty(r.Sum) && r.Status == ReturnStatus.PendingAccounting,
                _config.TaskDescriptions?.SentStatementToAccountingTask),

            (r => !string.IsNullOrEmpty(r.Sum) && r.Status == ReturnStatus.PendingReceipt,
                _config.TaskDescriptions?.ReceiptPrintedTask),
        ];

        var rowsList = rows.ToList();
        var sb = new StringBuilder();

        foreach ((Func<RowSheet, bool> filter, string? title) in sections)
            sb.Append(FormatMessage(rowsList.Where(filter), title));

        return sb.ToString();
    }

    private string FormatMessage(IEnumerable<RowSheet> rows, string? task)
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
