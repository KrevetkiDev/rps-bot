using BotRps.Application.Interfaces;
using BotRps.Infrastructure.Options;
using BotRps.Infrastructure.Persistence;
using BotRps.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(true);


builder.Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection("TelegramOptions"));
builder.Services.AddOptions<ResetBalanceOptions>()
    .Bind(builder.Configuration.GetSection("ResetBalanceOptions"));

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<ITelegramService, TelegramService>();
builder.Services.AddSingleton<IResetBalanceService, ResetBalanceService>();
builder.Services.AddHostedService<TelegramService>(p => (p.GetRequiredService<ITelegramService>() as TelegramService)!);
builder.Services.AddHostedService<ResetBalanceService>(p =>
    (p.GetRequiredService<IResetBalanceService>() as ResetBalanceService)!);
builder.Services.AddDbContextFactory<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));


var app = builder.Build();

var factory = app.Services.GetRequiredService<IDbContextFactory<DatabaseContext>>();
await using var context = factory.CreateDbContext();
await context.Database.MigrateAsync(app.Lifetime.ApplicationStopping);

app.Run();