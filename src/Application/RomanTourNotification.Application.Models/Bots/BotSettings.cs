namespace RomanTourNotification.Application.Models.Bots;

public class BotSettings
{
    public BotConfig NotificationBot { get; init; } = new();

    public BotConfig LoggingBot { get; init; } = new();
}