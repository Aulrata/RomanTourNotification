using Itmo.Dev.Platform.Persistence.Abstractions.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RomanTourNotification.Application.Abstractions.Persistence;
using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Groups;
using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Users;
using RomanTourNotification.Infrastructure.Persistence.Plugins;
using RomanTourNotification.Infrastructure.Persistence.Repositories.Groups;
using RomanTourNotification.Infrastructure.Persistence.Repositories.Users;

namespace RomanTourNotification.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection collection)
    {
        collection.AddPlatformPersistence(persistence => persistence
            .UsePostgres(postgres => postgres
                .WithConnectionOptions(b => b.BindConfiguration("Infrastructure:Persistence:Postgres"))
                .WithMigrationsFrom(typeof(IAssemblyMarker).Assembly)
                .WithDataSourcePlugin<MappingPlugin>()));

        // TODO: add repositories
        collection.AddScoped<IPersistenceContext, PersistenceContext>();

        collection.AddScoped<IUserRepository, UserRepository>();
        collection.AddScoped<IGroupRepository, GroupRepository>();

        return collection;
    }

    public static IServiceCollection AddHostedServices(this IServiceCollection collection)
    {
        return collection.AddHostedService<MigrationsBackgroundService>();
    }
}