using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Observability;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RomanTourNotification.Application.Extensions;
using RomanTourNotification.Infrastructure.Integrations.Extensions;
using RomanTourNotification.Infrastructure.Persistence.Extensions;
using RomanTourNotification.Presentation.TelegramBot.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddOptions<JsonSerializerSettings>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

builder.Services.AddPlatform();
builder.AddPlatformObservability();

builder.Services.AddApplication();
builder.Services.AddIntegrations(builder.Configuration);
builder.Services.AddTelegramBot();
builder.Services.AddInfrastructurePersistence();

builder.Services.AddHostedServices();
builder.Services.AddHostedApplicationServices();

builder.Services.AddUtcDateTimeProvider();

WebApplication app = builder.Build();

app.UseRouting();

app.UsePlatformObservability();

await app.RunAsync();
