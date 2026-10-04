using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.Messages;

/// <summary>Assembles notification messages for each group type.</summary>
public interface IMessageHandlerService
{
    /// <summary>Builds the "документы на вылет" message for a group.</summary>
    public Task<string> CreateDocumentsForDepartureMessageAsync(
        DateDto currentDay,
        Group group,
        CancellationToken cancellationToken);

    /// <summary>Builds the "авиабилеты" message for a group.</summary>
    public Task<string> CreateAirTicketsMessageAsync(
        DateDto currentDay,
        Group group,
        CancellationToken cancellationToken);

    /// <summary>Builds the "оплата" message for a group.</summary>
    public Task<string> CreatePaymentMessageAsync(
        DateDto currentDay,
        Group group,
        CancellationToken cancellationToken);
}
