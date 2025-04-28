using RomanTourNotification.Application.Models.EnrichmentNotification;

namespace RomanTourNotification.Application.Contracts.PaymentNotification;

public interface IPaymentNotificationService
{
    public Task<string> GetPaymentMessageAsync(DateDto dateDto, CancellationToken cancellationToken);
}