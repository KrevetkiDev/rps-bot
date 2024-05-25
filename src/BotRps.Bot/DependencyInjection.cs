using BotRpc.Bot.Pipes;
using BotRpc.Bot.Pipes.Base;
using BotRpc.Bot.Services;
using BotRps.Application.Common.Interfaces;

namespace BotRpc.Bot;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        return services.AddSingleton<ITelegramService, TelegramService>()
            .AddSingleton<IMessagePipe, BalanceMessagePipe>()
            .AddSingleton<IMessagePipe, BetDownMessagePipe>()
            .AddSingleton<IMessagePipe, BetUpMessagePipe>()
            .AddSingleton<IMessagePipe, PaperMessagePipe>()
            .AddSingleton<IMessagePipe, RockMessagePipe>()
            .AddSingleton<IMessagePipe, ScissorsMessagePipe>()
            .AddSingleton<IMessagePipe, StartMessagePipe>()
            .AddSingleton<IMessagePipe, ShowRatingMessagePipe>()
            .AddHostedService<TelegramService>(p => (p.GetRequiredService<ITelegramService>() as TelegramService)!);
    }
}