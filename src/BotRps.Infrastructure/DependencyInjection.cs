using BotRps.Application.Common.Interfaces;
using BotRps.Infrastructure.Options;
using BotRps.Infrastructure.Persistence;
using BotRps.Infrastructure.Persistence.Repository;
using BotRps.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotRps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddServices().AddSettings(configuration).AddDatabase(configuration);
    }

    private static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TelegramOptions>().Bind(configuration.GetSection("TelegramOptions"));
        services.AddOptions<ResetBalanceOptions>().Bind(configuration.GetSection("ResetBalanceOptions"));
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services.AddSingleton<IGameService, GameService>()
            .AddSingleton<ITelegramService, TelegramService>()
            .AddSingleton<IResetBalanceService, ResetBalanceService>()
            .AddSingleton<IRepository, Repository>()
            .AddHostedService<TelegramService>(p => (p.GetRequiredService<ITelegramService>() as TelegramService)!)
            .AddHostedService<ResetBalanceService>(p =>
                (p.GetRequiredService<IResetBalanceService>() as ResetBalanceService)!);
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddDbContextFactory<DatabaseContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));
    }
}