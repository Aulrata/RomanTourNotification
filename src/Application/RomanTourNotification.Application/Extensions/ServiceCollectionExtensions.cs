using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RomanTourNotification.Application.Abstractions.Time;
using RomanTourNotification.Application.Bots;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Messages;
using RomanTourNotification.Application.Contracts.Notifications;
using RomanTourNotification.Application.Contracts.NotificationService;
using RomanTourNotification.Application.Contracts.PaymentNotification;
using RomanTourNotification.Application.Contracts.ReceiptNotification;
using RomanTourNotification.Application.Contracts.ReturnNotification;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.DownloadData;
using RomanTourNotification.Application.DownloadData.Cache;
using RomanTourNotification.Application.EnrichmentNotification;
using RomanTourNotification.Application.Groups;
using RomanTourNotification.Application.Messages;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.GoogleSheets;
using RomanTourNotification.Application.Notifications;
using RomanTourNotification.Application.NotificationService;
using RomanTourNotification.Application.PaymentNotification;
using RomanTourNotification.Application.ReceiptNotification;
using RomanTourNotification.Application.ReturnNotification;
using RomanTourNotification.Application.Time;
using RomanTourNotification.Application.Users;

namespace RomanTourNotification.Application.Extensions;

/// <summary>Extension methods for registering application services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers all application-layer services.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddSingleton<IClock, SystemClock>();
        collection.AddSingleton<LoadDataCache>();

        collection.AddScoped<IUserService, UserService>();
        collection.AddScoped<IGroupService, GroupService>();

        collection.AddScoped<ILoadDataService, LoadDataService>(p =>
        {
            IEnumerable<ApiSettings> apiSettings = p.GetRequiredService<IOptions<List<ApiSettings>>>().Value;
            IGatewayService gateway = p.GetRequiredService<IGatewayService>();
            ILogger<LoadDataService> logger = p.GetRequiredService<ILogger<LoadDataService>>();
            LoadDataCache cache = p.GetRequiredService<LoadDataCache>();
            IClock clock = p.GetRequiredService<IClock>();
            return new LoadDataService(gateway, logger, apiSettings, cache, clock);
        });

        collection.AddScoped<ILoadEmployees, LoadEmployees>(p =>
        {
            IEnumerable<ApiSettings> apiSettings = p.GetRequiredService<IOptions<List<ApiSettings>>>().Value;
            IGatewayService gateway = p.GetRequiredService<IGatewayService>();
            ILogger<LoadEmployees> logger = p.GetRequiredService<ILogger<LoadEmployees>>();
            return new LoadEmployees(gateway, apiSettings, logger);
        });

        collection.AddScoped<ILoadBills, LoadBills>(p =>
        {
            IEnumerable<ApiSettings> apiSettings = p.GetRequiredService<IOptions<List<ApiSettings>>>().Value;
            IGatewayService gateway = p.GetRequiredService<IGatewayService>();
            ILogger<LoadBills> logger = p.GetRequiredService<ILogger<LoadBills>>();
            return new LoadBills(logger, gateway, apiSettings);
        });

        collection.AddScoped<SheetsService>(p =>
        {
            GoogleSheetsConfig config = p.GetRequiredService<IOptions<GoogleSheetsConfig>>().Value;

            using var stream = new FileStream(config.ConfigPath, FileMode.Open, FileAccess.Read);

            GoogleCredential credential = ServiceAccountCredential
                .FromServiceAccountData(stream)
                .ToGoogleCredential()
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
            ILogger<ReturnNotificationService> logger =
                p.GetRequiredService<ILogger<ReturnNotificationService>>();
            return new ReturnNotificationService(sheetsService, config, logger);
        });

        collection.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
        collection.AddScoped<IEnrichmentNotificationService, EnrichmentNotificationService>();
        collection.AddScoped<IReceiptNotificationService, ReceiptNotificationService>();
        collection.AddScoped<IMessageHandlerService, MessageHandlerService>();
        collection.AddScoped<IFilterEnrichmentNotificationService, FilterEnrichmentNotificationService>();

        RegisterNotifications(collection);

        collection.AddScoped<INotificationService, TelegramService>();

        return collection;
    }

    /// <summary>Registers background hosted application services.</summary>
    public static IServiceCollection AddHostedApplicationServices(this IServiceCollection collection)
    {
        return collection.AddHostedService<NotificationsBackgroundService>();
    }

    /// <summary>
    /// Registers each notification class once under its concrete type, then maps it to both
    /// <see cref="IScheduledNotification"/> and (where applicable) <see cref="IForcedNotification"/>
    /// via factory delegates — ensuring a single instance per scope regardless of how it is resolved.
    /// </summary>
    private static void RegisterNotifications(IServiceCollection collection)
    {
        // Documents for departure — weekdays, main time
        collection.AddScoped<DocumentsForDepartureNotification>();
        collection.AddScoped<IScheduledNotification>(p => p.GetRequiredService<DocumentsForDepartureNotification>());
        collection.AddScoped<IForcedNotification>(p => p.GetRequiredService<DocumentsForDepartureNotification>());

        // Air tickets — every day, main time
        collection.AddScoped<AirTicketsNotification>();
        collection.AddScoped<IScheduledNotification>(p => p.GetRequiredService<AirTicketsNotification>());
        collection.AddScoped<IForcedNotification>(p => p.GetRequiredService<AirTicketsNotification>());

        // Payment — weekdays, main time
        collection.AddScoped<PaymentGroupNotification>();
        collection.AddScoped<IScheduledNotification>(p => p.GetRequiredService<PaymentGroupNotification>());
        collection.AddScoped<IForcedNotification>(p => p.GetRequiredService<PaymentGroupNotification>());

        // Daily return — weekdays, main time; also handles forced Return sends from bot
        collection.AddScoped<DailyReturnNotification>();
        collection.AddScoped<IScheduledNotification>(p => p.GetRequiredService<DailyReturnNotification>());
        collection.AddScoped<IForcedNotification>(p => p.GetRequiredService<DailyReturnNotification>());

        // Special return — Wednesday only, return time; no forced variant
        collection.AddScoped<SpecialReturnNotification>();
        collection.AddScoped<IScheduledNotification>(p => p.GetRequiredService<SpecialReturnNotification>());

        // Receipt — weekdays, receipt time
        collection.AddScoped<ReceiptGroupNotification>();
        collection.AddScoped<IScheduledNotification>(p => p.GetRequiredService<ReceiptGroupNotification>());
        collection.AddScoped<IForcedNotification>(p => p.GetRequiredService<ReceiptGroupNotification>());
    }
}
