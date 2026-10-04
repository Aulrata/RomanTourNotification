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

            if (row[0].ToString() == "Итого:")
                break;

            RowSheet? rowData = GetRow(row);

            if (rowData is null)
                continue;

            rowSheets.Add(rowData);
        }

        return rowSheets.Skip(1);
    }

    /// <summary>
    /// Missing index is treated as false — Sheets omits trailing empty cells from the row.
    /// </summary>
    private static bool GetBoolValue(IList<object> row, int index)
    {
        return index >= 0 && index < row.Count && row[index].ToString() == "TRUE";
    }

    /// <summary>
    /// Missing index is treated as empty — Sheets omits trailing empty cells from the row.
    /// </summary>
    private static string GetStringValue(IList<object> row, int index)
    {
        if (index < 0 || index >= row.Count)
            return string.Empty;

        return row[index].ToString()?.Trim() ?? string.Empty;
    }

    private RowSheet? GetRow(IList<object>? row)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (_sheetsConfig.ColumIndex is null)
        {
            _logger.LogError("Invalid colum index. Null");
            return null;
        }

        // Google Sheets Values API omits trailing empty cells, so a row with empty ReturnedSum
        // may be shorter than MinimumColumns. Require only columns up to (not including) ReturnedSum.
        int minimumRequired = Math.Min(
            _sheetsConfig.ColumIndex.MinimumColumns,
            _sheetsConfig.ColumIndex.ReturnedSum);

        if (row.Count < minimumRequired)
        {
            _logger.LogWarning(
                "Row count is less than minimum columns. Minimum {MinimumColumns}, current {Count}",
                minimumRequired,
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
                GetBoolValue(row, _sheetsConfig.ColumIndex.SentStatementToAccounting),
                GetBoolValue(row, _sheetsConfig.ColumIndex.ReceiptPrinted),
                GetStringValue(row, _sheetsConfig.ColumIndex.SentApplicationToTourOperator),
                GetStringValue(row, _sheetsConfig.ColumIndex.ReturnedSum));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при парсинге данных. {ex.Message} ", ex);
        }
    }
}