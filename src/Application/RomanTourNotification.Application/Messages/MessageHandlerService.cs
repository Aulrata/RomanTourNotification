using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Messages;
using RomanTourNotification.Application.Contracts.PaymentNotification;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;
using System.Text;

namespace RomanTourNotification.Application.Messages;

public class MessageHandlerService : IMessageHandlerService
{
    private readonly ILogger<MessageHandlerService> _logger;
    private readonly IEnrichmentNotificationService _enrichmentNotificationService;
    private readonly IPaymentNotificationService _paymentNotificationService;

    public MessageHandlerService(
        IEnrichmentNotificationService enrichmentNotificationService,
        ILogger<MessageHandlerService> logger,
        IPaymentNotificationService paymentNotificationService)
    {
        _enrichmentNotificationService = enrichmentNotificationService;
        _logger = logger;
        _paymentNotificationService = paymentNotificationService;
    }

    /// <inheritdoc/>
    public async Task<string> CreateDocumentsForDepartureMessageAsync(
        DateDto currentDay,
        Group group,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating documents-for-departure message for group: {Title}", group.Title);
        var sb = new StringBuilder();

        await _enrichmentNotificationService.GetDocumentsForDepartureAsync(currentDay, sb, group.ManagerFullname, cancellationToken);

        if (currentDay.From.DayOfWeek is DayOfWeek.Friday)
        {
            var saturday = new DateDto(currentDay.From.AddDays(1));
            await _enrichmentNotificationService.GetDocumentsForDepartureAsync(saturday, sb, group.ManagerFullname, cancellationToken);

            var sunday = new DateDto(currentDay.From.AddDays(2));
            await _enrichmentNotificationService.GetDocumentsForDepartureAsync(sunday, sb, group.ManagerFullname, cancellationToken);
        }

        _logger.LogInformation("Documents-for-departure message created for group: {Title}", group.Title);
        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task<string> CreateAirTicketsMessageAsync(
        DateDto currentDay,
        Group group,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating air-tickets message for group: {Title}", group.Title);
        var sb = new StringBuilder();

        // Air tickets run every day, so no Friday weekend preview needed —
        // Saturday and Sunday notifications are sent automatically on those days.
        await _enrichmentNotificationService.GetAirTicketsAsync(currentDay, sb, group.ManagerFullname, cancellationToken);

        _logger.LogInformation("Air-tickets message created for group: {Title}", group.Title);
        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task<string> CreatePaymentMessageAsync(DateDto currentDay, Group group, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating payment message for group: {Title}", group.Title);
        var sb = new StringBuilder();
        string greetings = GetMessageDate(currentDay);

        sb.AppendLine(greetings);

        await _paymentNotificationService.GetPaymentMessageAsync(currentDay, sb, group.ManagerFullname, cancellationToken);

        if (currentDay.From.DayOfWeek is DayOfWeek.Friday)
            await CreatePaymentWeekendMessageAsync(sb, currentDay, group.ManagerFullname, cancellationToken);

        _logger.LogInformation("Payment message created for group: {Title}", group.Title);
        return sb.ToString();
    }

    private static string GetMessageDate(DateDto currentDay)
    {
        return $"""

                 <b><u>Доплата туристов на {currentDay.From.Date:dd.MM.yyyy}</u></b>.

                 """;
    }

    private async Task CreatePaymentWeekendMessageAsync(
        StringBuilder sb,
        DateDto currentDay,
        string managerFullname,
        CancellationToken cancellationToken)
    {
        var saturdayDay = new DateDto(currentDay.From.AddDays(1));
        sb.Append(GetMessageDate(saturdayDay));
        await _paymentNotificationService.GetPaymentMessageAsync(saturdayDay, sb, managerFullname, cancellationToken);

        var sundayDay = new DateDto(currentDay.From.AddDays(2));
        sb.Append(GetMessageDate(sundayDay));
        await _paymentNotificationService.GetPaymentMessageAsync(sundayDay, sb, managerFullname, cancellationToken);
    }
}
