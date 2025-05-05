using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RomanTourNotification.Application.Bots;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.PaymentNotification;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.DownloadData;
using RomanTourNotification.Application.EnrichmentNotification;
using RomanTourNotification.Application.Groups;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.PaymentNotification;
using RomanTourNotification.Application.Users;

namespace RomanTourNotification.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddSingleton<IUserService, UserService>();
        collection.AddSingleton<IGroupService, GroupService>();
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

        collection.AddScoped<IPaymentNotificationService, PaymentNotificationService>();

        collection.AddScoped<IEnrichmentNotificationService, EnrichmentNotificationService>();
        return collection;
    }

    public static IServiceCollection AddHostedApplicationServices(this IServiceCollection collection)
    {
        return collection.AddHostedService<NotificationsBackgroundService>();
    }
}