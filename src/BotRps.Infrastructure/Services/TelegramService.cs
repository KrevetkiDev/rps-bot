using BotRpc.Domain.Enums;
using BotRps.Application;
using BotRps.Application.Extensions;
using BotRps.Application.Interfaces;
using BotRps.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotRps.Infrastructure.Services;

public class TelegramService : ITelegramService, IHostedService
{
    private readonly ITelegramBotClient _client;
    private readonly IGameService _gameService;

    public TelegramService(IOptions<TelegramOptions> options, IGameService gameService)
    {
        _client = new TelegramBotClient(options.Value.Token);
        _gameService = gameService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.StartReceiving(UpdateHandler, PollingErrorHandler, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message != null)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Делай ход: к, н, б?");
            }
        
            if (message.Text == "к" || message.Text == "н" || message.Text == "б")
            {
                var messageNew = RpsItemParser.ParseToRps(message.Text);
                if (messageNew.HasValue)
                {
                    var result = _gameService.Game(messageNew.Value);
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        $"Бот выбрал: {RpsItemsExtensions.ToRuLetter(result.BotChoice)}. {GameResultTypesExtensions.ToRuString(result.Type)}.  ");
                }
            }
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        return Task.CompletedTask;
    }
}