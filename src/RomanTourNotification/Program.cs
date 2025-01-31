using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Observability;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RomanTourNotification.Application.Contracts.Bots;
using RomanTourNotification.Application.Extensions;
using RomanTourNotification.Infrastructure.Persistence.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddOptions<JsonSerializerSettings>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

builder.Services.AddPlatform();
builder.AddPlatformObservability();

builder.Services.AddApplication();
builder.Services.AddBotExtensions(builder.Configuration);

builder.Services.AddInfrastructurePersistence();

builder.Services.AddUtcDateTimeProvider();

WebApplication app = builder.Build();

INotificationBotService notificationBot = app.Services.GetRequiredService<INotificationBotService>();

using var cts = new CancellationTokenSource();
await notificationBot.StartAsync(cts.Token);

app.UseRouting();

app.UsePlatformObservability();

await app.RunAsync();