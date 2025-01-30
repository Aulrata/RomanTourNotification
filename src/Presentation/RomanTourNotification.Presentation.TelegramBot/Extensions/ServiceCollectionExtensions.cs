using Microsoft.Extensions.DependencyInjection;

namespace RomanTourNotification.Presentation.TelegramBot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection collection)
    {
        return collection;
    }
}