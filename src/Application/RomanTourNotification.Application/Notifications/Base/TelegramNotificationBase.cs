using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Models.Groups;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace RomanTourNotification.Application.Notifications.Base;

/// <summary>
/// Shared infrastructure for all Telegram-based notifications:
/// message truncation, sending, and uniform error logging.
/// </summary>
public abstract class TelegramNotificationBase
{
    private const int MaxMessageLength = 8096;
    private const int TruncatedMessageLength = 4093;

    private readonly ILogger _logger;

    /// <summary>Gets the Telegram bot client used to send messages.</summary>
    protected ITelegramBotClient BotClient { get; }

    /// <summary>Initializes a new instance of the <see cref="TelegramNotificationBase"/> class.</summary>
    protected TelegramNotificationBase(ILogger logger, ITelegramBotClient botClient)
    {
        _logger = logger;
        BotClient = botClient;
    }

    /// <summary>Sends <paramref name="message"/> to <paramref name="group"/>, truncating if necessary.</summary>
    protected async Task SendMessageAsync(string message, Group group, CancellationToken cancellationToken)
    {
        try
        {
            if (message.Length > MaxMessageLength)
            {
                _logger.LogWarning("Message too long ({Length} chars), truncating", message.Length);
                message = message[..TruncatedMessageLength] + "...";
            }

            await BotClient.SendMessage(
                group.ChatId,
                message,
                cancellationToken: cancellationToken,
                parseMode: ParseMode.Html);

            _logger.LogInformation("Notification sent to group: {Title}", group.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send notification to group: {Title} (ChatId: {ChatId}). {InnerException}",
                group.Title,
                group.ChatId,
                ex.InnerException?.Message ?? "None");
        }
    }
}
