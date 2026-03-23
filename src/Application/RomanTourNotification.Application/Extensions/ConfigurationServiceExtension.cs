using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Gateway;
using RomanTourNotification.Application.Models.Bots;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using RomanTourNotification.Application.Models.GoogleSheets;
using Telegram.Bot;

namespace RomanTourNotification.Application.Extensions;

public static class ConfigurationServiceExtension
{
    public static IServiceCollection AddHttpClientConfigurationService(this IServiceCollection collection, IConfiguration configuration)
    {
        try
        {
            collection.Configure<BotSettings>(configuration.GetSection("BotSettings"));
            var httpclient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(300),
            };
            collection.AddSingleton<ITelegramBotClient>(provide =>
            {
                BotSettings botSettings = provide.GetRequiredService<IOptions<BotSettings>>().Value;
                return new TelegramBotClient(botSettings.NotificationBot.Token, httpclient);
            });

            collection.Configure<ConfigurationService>(configuration.GetSection("ConfigurationService"));
            collection.Configure<TimeSettings>(configuration.GetSection("TimeSettings"));
            collection.Configure<GoogleSheetsConfig>(configuration.GetSection("GoogleSheetsConfig"));

            collection.AddSingleton<TimeSettings>(provide =>
            {
                TimeSettings timeSettings = provide.GetRequiredService<IOptions<TimeSettings>>().Value;
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

            collection.Configure<List<ApiSettings>>(configuration.GetSection("ApiSettings"));

            collection.AddHttpClient();
            collection.AddScoped<IGatewayService, GatewayService>(provider =>
            {
                IHttpClientFactory httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = httpClientFactory.CreateClient();

                ConfigurationService configurationService = provider.GetRequiredService<IOptions<ConfigurationService>>().Value;

                httpClient.BaseAddress = new Uri(configurationService.BaseAddress ?? string.Empty);
                ILogger<GatewayService> logger = provider.GetRequiredService<ILogger<GatewayService>>();
                return new GatewayService(httpClient, logger);
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return collection;
    }
}