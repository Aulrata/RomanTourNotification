using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RomanTourNotification.Application.Bots;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Messages;
using RomanTourNotification.Application.Contracts.NotificationService;
using RomanTourNotification.Application.Contracts.PaymentNotification;
using RomanTourNotification.Application.Contracts.ReturnNotification;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.DownloadData;
using RomanTourNotification.Application.EnrichmentNotification;
using RomanTourNotification.Application.Groups;
using RomanTourNotification.Application.Messages;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.GoogleSheets;
using RomanTourNotification.Application.NotificationService;
using RomanTourNotification.Application.PaymentNotification;
using RomanTourNotification.Application.ReturnNotification;
using RomanTourNotification.Application.Users;

namespace RomanTourNotification.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddScoped<IUserService, UserService>();
        collection.AddScoped<IGroupService, GroupService>();
        collection.AddScoped<ILoadDataService, LoadDataService>(p =>
        {
            IEnumerable<ApiSettings> apiSettings = p.GetRequiredService<IOptions<List<ApiSettings>>>().Value;
            IGatewayService gateway = p.GetRequiredService<IGatewayService>();
            ILogger<LoadDataService> logger =
                p.GetRequiredService<ILogger<LoadDataService>>();

            return new LoadDataService(gateway, logger, apiSettings);
        });
        collection.AddScoped<ILoadEmployees, LoadEmployees>(p =>
        {
            IEnumerable<ApiSettings> apiSettings = p.GetRequiredService<IOptions<List<ApiSettings>>>().Value;
            IGatewayService gateway = p.GetRequiredService<IGatewayService>();
            ILogger<LoadEmployees> logger = p.GetRequiredService<ILogger<LoadEmployees>>();

            return new LoadEmployees(gateway, apiSettings, logger);
        });

        collection.AddScoped<SheetsService>(p =>
        {
            GoogleSheetsConfig config = p.GetRequiredService<IOptions<GoogleSheetsConfig>>().Value;

            using var stream = new FileStream(config.ConfigPath, FileMode.Open, FileAccess.Read);

            GoogleCredential credential = ServiceAccountCredential.FromServiceAccountData(stream).ToGoogleCredential()
                .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = config.Name,
            });
        });

        collection.AddScoped<ILoadSheetData, LoadSheetData>(p =>
        {
            GoogleSheetsConfig config = p.GetRequiredService<IOptions<GoogleSheetsConfig>>().Value;
            SheetsService sheetsService = p.GetRequiredService<SheetsService>();
            ILogger<LoadSheetData> logger = p.GetRequiredService<ILogger<LoadSheetData>>();

            return new LoadSheetData(config, sheetsService, logger);
        });

        collection.AddScoped<IReturnNotificationService, ReturnNotificationService>(p =>
        {
            GoogleSheetsConfig config = p.GetRequiredService<IOptions<GoogleSheetsConfig>>().Value;
            ILoadSheetData sheetsService = p.GetRequiredService<ILoadSheetData>();
            ILogger<ReturnNotificationService> logger = p.GetRequiredService<ILogger<ReturnNotificationService>>();

            return new ReturnNotificationService(sheetsService, config, logger);
        });

        collection.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
        collection.AddScoped<IEnrichmentNotificationService, EnrichmentNotificationService>();
        collection.AddScoped<INotificationService, TelegramService>();
        collection.AddScoped<IMessageHandlerService, MessageHandlerService>();
        collection.AddScoped<IFilterEnrichmentNotificationService, FilterEnrichmentNotificationService>();
        return collection;
    }

    public static IServiceCollection AddHostedApplicationServices(this IServiceCollection collection)
    {
        return collection.AddHostedService<NotificationsBackgroundService>();
    }
}