using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.Messages;

public interface IMessageHandlerService
{
    public Task<string> CreateArrivalMessageAsync(DateDto currentDay, CancellationToken cancellationToken);

    public Task<string> CreatePaymentMessageAsync(DateDto currentDay, Group group, CancellationToken cancellationToken);
}