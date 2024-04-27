using BotRps.Application.Interfaces;
using BotRps.Infrastructure.Options;
using BotRps.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(true);


builder.Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection("TelegramOptions"));

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<ITelegramService, TelegramService>();
builder.Services.AddHostedService<TelegramService>(p => (p.GetRequiredService<ITelegramService>() as TelegramService)!);


var app = builder.Build();

app.Run();