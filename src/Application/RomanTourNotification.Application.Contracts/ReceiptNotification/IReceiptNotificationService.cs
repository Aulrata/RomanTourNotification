using RomanTourNotification.Application.Models.EnrichmentNotification;

namespace RomanTourNotification.Application.Contracts.ReceiptNotification;

public interface IReceiptNotificationService
{
    public Task<string> GetReceiptMessageAsync(DateDto dateDto, CancellationToken cancellationToken);
}