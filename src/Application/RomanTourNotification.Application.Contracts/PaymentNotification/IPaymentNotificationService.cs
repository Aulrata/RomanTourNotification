using RomanTourNotification.Application.Models.EnrichmentNotification;
using System.Text;

namespace RomanTourNotification.Application.Contracts.PaymentNotification;

public interface IPaymentNotificationService
{
    public Task GetPaymentMessageAsync(
        DateDto currentDay,
        StringBuilder sb,
        string managerFullname,
        CancellationToken cancellationToken);
}