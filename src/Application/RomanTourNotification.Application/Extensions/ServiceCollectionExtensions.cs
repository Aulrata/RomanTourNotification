using Microsoft.Extensions.DependencyInjection;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Users;

namespace RomanTourNotification.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        // TODO: add services
        collection.AddSingleton<IUserService, UserService>();
        return collection;
    }
}