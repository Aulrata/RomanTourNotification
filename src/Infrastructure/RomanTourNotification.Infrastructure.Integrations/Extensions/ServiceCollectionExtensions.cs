using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Models.Bots;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using RomanTourNotification.Application.Models.GoogleSheets;
using RomanTourNotification.Infrastructure.Integrations.Gateway;
using Telegram.Bot;

namespace RomanTourNotification.Infrastructure.Integrations.Extensions;

/// <summary>
/// Extension methods for registering external integration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Telegram bot client, HTTP gateway service, and external configuration sections.
    /// </summary>
    public static IServiceCollection AddIntegrations(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        RegisterTelegram(collection, configuration);
        RegisterGateway(collection, configuration);
        collection.Configure<GoogleSheetsConfig>(configuration.GetSection("GoogleSheetsConfig"));
        return collection;
    }

    private static void RegisterTelegram(IServiceCollection collection, IConfiguration configuration)
    {
        collection.Configure<BotSettings>(configuration.GetSection("BotSettings"));

        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };

        collection.AddSingleton<ITelegramBotClient>(provider =>
        {
            BotSettings botSettings = provider.GetRequiredService<IOptions<BotSettings>>().Value;
            return new TelegramBotClient(botSettings.NotificationBot.Token, httpClient);
        });
    }

    private static void RegisterGateway(IServiceCollection collection, IConfiguration configuration)
    {
        collection.Configure<ConfigurationService>(configuration.GetSection("ConfigurationService"));
        collection.Configure<TimeSettings>(configuration.GetSection("TimeSettings"));
        collection.Configure<List<ApiSettings>>(configuration.GetSection("ApiSettings"));

        collection.AddSingleton<TimeSettings>(provider =>
        {
            TimeSettings timeSettings = provider.GetRequiredService<IOptions<TimeSettings>>().Value;
            return new TimeSettings
            {
                HoursUtc = timeSettings.HoursUtc,
                Minutes = timeSettings.Minutes,
                ReturnHoursUtc = timeSettings.ReturnHoursUtc,
                ReturnMinute = timeSettings.ReturnMinute,
                ReceiptHoursUtc = timeSettings.ReceiptHoursUtc,
                ReceiptMinutes = timeSettings.ReceiptMinutes,
            };
        });

        collection.AddHttpClient();
        collection.AddScoped<IGatewayService, GatewayService>(provider =>
        {
            IHttpClientFactory httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = httpClientFactory.CreateClient();

            ConfigurationService configurationService =
                provider.GetRequiredService<IOptions<ConfigurationService>>().Value;
            httpClient.BaseAddress = new Uri(configurationService.BaseAddress ?? string.Empty);

            ILogger<GatewayService> logger = provider.GetRequiredService<ILogger<GatewayService>>();
            return new GatewayService(httpClient, logger);
        });
    }
}
