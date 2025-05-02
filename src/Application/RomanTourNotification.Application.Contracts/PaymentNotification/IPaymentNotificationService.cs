using RomanTourNotification.Application.Models.EnrichmentNotification;
using System.Text;

namespace RomanTourNotification.Application.Contracts.PaymentNotification;

public interface IPaymentNotificationService
{
    public Task GetAllPaymentMessagesAsync(DateDto dateDto, StringBuilder sb, CancellationToken cancellationToken);

    public Task GetPaymentMessageByManagerAsync(DateDto dateDto, StringBuilder sb, string managerFullname, CancellationToken cancellationToken);
}