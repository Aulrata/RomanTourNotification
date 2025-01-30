using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RomanTourNotification.Application.Models.Bots;
using Telegram.Bot;

namespace RomanTourNotification.Application.Extensions;

public static class BotExtensions
{
    public static IServiceCollection AddBotExtensions(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.Configure<BotSettings>(configuration.GetSection("BotSettings"));

        collection.AddSingleton<ITelegramBotClient>(provide =>
        {
            BotSettings botSettings = provide.GetRequiredService<IOptions<BotSettings>>().Value;
            return new TelegramBotClient(botSettings.NotificationBot.Token);
        });

        return collection;
    }
}