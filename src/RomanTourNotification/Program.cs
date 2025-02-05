using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Observability;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RomanTourNotification.Application.Extensions;
using RomanTourNotification.Infrastructure.Persistence.Extensions;
using RomanTourNotification.Presentation.TelegramBot.Extensions;
using RomanTourNotification.Presentation.TelegramBot.Receiving;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddOptions<JsonSerializerSettings>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

builder.Services.AddPlatform();
builder.AddPlatformObservability();

builder.Services.AddApplication();
builder.Services.AddBotExtensions(builder.Configuration);
builder.Services.AddTelegramBot();
builder.Services.AddInfrastructurePersistence();

builder.Services.AddHostedServices();
builder.Services.AddHostedApplicationServices();

builder.Services.AddUtcDateTimeProvider();

WebApplication app = builder.Build();
NotificationBotReceiving? notificationBot = null;

try
{
    notificationBot = app.Services.GetRequiredService<NotificationBotReceiving>();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Program Error: {ex.Message}");
    Console.ForegroundColor = ConsoleColor.Gray;
}

using var cts = new CancellationTokenSource();

if (notificationBot is not null) await notificationBot.StartAsync(cts.Token);
app.UseRouting();

app.UsePlatformObservability();

Console.WriteLine("App started");
await app.RunAsync();
