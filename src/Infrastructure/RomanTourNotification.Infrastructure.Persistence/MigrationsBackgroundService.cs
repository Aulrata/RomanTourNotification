using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RomanTourNotification.Infrastructure.Persistence;

public class MigrationsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationsBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

        IServiceProvider services = scope.ServiceProvider;
        IMigrationRunner runner = services.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();

        await Task.CompletedTask;
    }
}