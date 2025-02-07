using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RomanTourNotification.Application.Bots;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.EnrichmentNotification;
using RomanTourNotification.Application.Groups;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Users;

namespace RomanTourNotification.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        // TODO: add services
        collection.AddSingleton<IUserService, UserService>();
        collection.AddSingleton<IGroupService, GroupService>();
        collection.AddScoped<IEnrichmentNotificationService, EnrichmentNotificationService>(provider =>
        {
            IEnumerable<ApiSettings> apiSettings = provider.GetRequiredService<IOptions<List<ApiSettings>>>().Value;
            IGatewayService gateway = provider.GetRequiredService<IGatewayService>();

            return new EnrichmentNotificationService(apiSettings, gateway);
        });
        return collection;
    }

    public static IServiceCollection AddHostedApplicationServices(this IServiceCollection collection)
    {
        return collection.AddHostedService<NotificationsBackgroundService>();
    }
}