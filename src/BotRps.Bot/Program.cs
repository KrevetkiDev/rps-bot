using BotRps.Application.Interfaces;
using BotRps.Infrastructure.Options;
using BotRps.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection("TelegramOptions"));

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<ITelegramService, TelegramService>();

var app = builder.Build();

app.Run();