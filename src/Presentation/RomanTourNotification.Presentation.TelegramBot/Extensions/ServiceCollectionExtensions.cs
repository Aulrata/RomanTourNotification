using Microsoft.Extensions.DependencyInjection;
using RomanTourNotification.Presentation.TelegramBot.Receiving;

namespace RomanTourNotification.Presentation.TelegramBot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection collection)
    {
        collection.AddSingleton<NotificationBotReceiving>();
        return collection;
    }
}