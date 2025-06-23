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

    public async Task<string> CreateArrivalMessageAsync(DateDto currentDay, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating arrival message");
        var sb = new StringBuilder();

        await _enrichmentNotificationService.GetArrivalByDateAsync(currentDay, sb, cancellationToken);

        if (currentDay.From.DayOfWeek is DayOfWeek.Friday)
            await CreateArrivalWeekendMessageAsync(currentDay, sb, cancellationToken);

        _logger.LogInformation("Arrival message created");
        return sb.ToString();
    }

    public async Task<string> CreatePaymentMessageAsync(DateDto currentDay, Group group, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Creating payment message for group: {group.Title}");
        var sb = new StringBuilder();
        string greetings = $"""
                             Доброе утро!
                            Доплата туристов на {currentDay.From.Date:dd.MM.yyyy}.

                            """;

        sb.AppendLine(greetings);

        await _paymentNotificationService.GetPaymentMessageAsync(currentDay, sb, group.ManagerFullname, cancellationToken);

        if (currentDay.From.DayOfWeek is DayOfWeek.Friday)
            await CreatePaymentWeekendMessageAsync(sb, currentDay, group.ManagerFullname, cancellationToken);

        _logger.LogInformation($"Payment message created for group: {group.Title}");
        return sb.ToString();
    }

    private async Task CreateArrivalWeekendMessageAsync(
        DateDto currentDay,
        StringBuilder sb,
        CancellationToken cancellationToken)
    {
        var saturdayDay = new DateDto(currentDay.From.AddDays(1));
        await _enrichmentNotificationService.GetArrivalByDateAsync(saturdayDay, sb, cancellationToken);

        var sundayDay = new DateDto(currentDay.From.AddDays(2));
        await _enrichmentNotificationService.GetArrivalByDateAsync(sundayDay, sb, cancellationToken);
    }

    private async Task CreatePaymentWeekendMessageAsync(
        StringBuilder sb,
        DateDto currentDay,
        string managerFullname,
        CancellationToken cancellationToken)
    {
        var saturdayDay = new DateDto(currentDay.From.AddDays(1));
        string saturdayGreetings = $"""

                                    Доплата туристов на {saturdayDay.From.Date:dd.MM.yyyy}.


                                    """;
        sb.Append(saturdayGreetings);

        await _paymentNotificationService.GetPaymentMessageAsync(saturdayDay, sb, managerFullname, cancellationToken);

        var sundayDay = new DateDto(currentDay.From.AddDays(2));
        string sundayGreetings = $"""

                                  Доплата туристов на {sundayDay.From.Date:dd.MM.yyyy}.


                                  """;
        sb.Append(sundayGreetings);

        await _paymentNotificationService.GetPaymentMessageAsync(sundayDay, sb, managerFullname, cancellationToken);
    }
}