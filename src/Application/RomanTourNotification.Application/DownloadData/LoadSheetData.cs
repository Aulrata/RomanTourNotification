using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Models.GoogleSheets;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace RomanTourNotification.Application.DownloadData;

public class LoadSheetData : ILoadSheetData
{
    private readonly GoogleSheetsConfig _sheetsConfig;
    private readonly ILogger<LoadSheetData> _logger;
    private readonly SheetsService _sheetsService;

    public LoadSheetData(GoogleSheetsConfig sheetsConfig, SheetsService sheetsService, ILogger<LoadSheetData> logger)
    {
        _sheetsConfig = sheetsConfig;
        _sheetsService = sheetsService;
        _logger = logger;
    }

    public async Task<IEnumerable<RowSheet>> GetRowSheetsAsync(CancellationToken cancellationToken)
    {
        GetRequest request =
            _sheetsService.Spreadsheets.Values.Get(_sheetsConfig.Id, $"{_sheetsConfig.List}!{_sheetsConfig.Range}");
        ValueRange response = await request.ExecuteAsync(cancellationToken);

        IList<IList<object>>? values = response.Values;

        if (values is null || values.Count == 0)
        {
            _logger.LogWarning("Google Sheets returned no data for range {List}!{Range}", _sheetsConfig.List, _sheetsConfig.Range);
            return [];
        }

        List<RowSheet> rowSheets = [];

        foreach (IList<object> row in values)
        {
            if (row.Count <= 0 || string.IsNullOrEmpty(row[0].ToString()))
                continue;

            RowSheet? rowData = GetRow(row);

            if (rowData is null)
                continue;

            rowSheets.Add(rowData);
        }

        return rowSheets.Skip(1);
    }

    private RowSheet? GetRow(IList<object>? row)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (_sheetsConfig.ColumIndex is null)
        {
            _logger.LogError("Invalid colum index. Null");
            return null;
        }

        if (row.Count < _sheetsConfig.ColumIndex.MinimumColumns)
        {
            _logger.LogWarning(
                "Row count is less than minimum columns. Minimum {MinimumColumns}, current {Count}",
                _sheetsConfig.ColumIndex.MinimumColumns,
                row.Count);

            return null;
        }

        try
        {
            return new RowSheet(
                GetStringValue(row, _sheetsConfig.ColumIndex.LastName),
                GetStringValue(row, _sheetsConfig.ColumIndex.Sum),
                GetStringValue(row, _sheetsConfig.ColumIndex.Manager),
                GetStringValue(row, _sheetsConfig.ColumIndex.Organization),
                GetBoolValue(row, _sheetsConfig.ColumIndex.SentStatementToTourist),
                GetBoolValue(row, _sheetsConfig.ColumIndex.GetStatementFromTourist),
                GetOptionalBoolValue(row, _sheetsConfig.ColumIndex.SentStatementToAccounting),
                GetOptionalBoolValue(row, _sheetsConfig.ColumIndex.ReceiptPrinted),
                GetOptionalBoolValue(row, _sheetsConfig.ColumIndex.SentApplicationToTourOperator));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при парсинге данных. {ex.Message} ", ex);
        }
    }

    private bool GetBoolValue(IList<object> row, int index)
    {
        return row[index].ToString() == "TRUE";
    }

    /// <summary>Returns false when the column has not yet been added to the sheet.</summary>
    private bool GetOptionalBoolValue(IList<object> row, int index)
    {
        return index >= 0 && index < row.Count && row[index].ToString() == "TRUE";
    }

    private string GetStringValue(IList<object> row, int index)
    {
        return row[index].ToString()?.Trim() ?? string.Empty;
    }
}