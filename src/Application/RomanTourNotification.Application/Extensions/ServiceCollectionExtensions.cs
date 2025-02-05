using Microsoft.Extensions.DependencyInjection;
using RomanTourNotification.Application.Bots;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Groups;
using RomanTourNotification.Application.Users;

namespace RomanTourNotification.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        // TODO: add services
        collection.AddSingleton<IUserService, UserService>();
        collection.AddSingleton<IGroupService, GroupService>();
        return collection;
    }

    public static IServiceCollection AddHostedApplicationServices(this IServiceCollection collection)
    {
        return collection.AddHostedService<NotificationsBackgroundService>();
    }
}