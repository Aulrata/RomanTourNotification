using RomanTourNotification.Application.Models.GoogleSheets;

namespace RomanTourNotification.Application.Contracts.DownloadData;

public interface ILoadSheetData
{
    public Task<IEnumerable<RowSheet>> GetRowSheetsAsync(CancellationToken cancellationToken);
}