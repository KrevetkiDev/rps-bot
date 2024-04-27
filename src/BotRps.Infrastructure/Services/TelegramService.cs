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
        var playerChoice = update.Message;
        if (playerChoice != null)
        {
            if (playerChoice.Text == "/start")
            {
                await botClient.SendTextMessageAsync(playerChoice.Chat.Id, "Делай ход: к, н, б?", cancellationToken: cancellationToken);
            }
        
            if (playerChoice.Text == "к" || playerChoice.Text == "н" || playerChoice.Text == "б")
            {
                var messageNew = RpsItemParser.ParseToRps(playerChoice.Text);
                if (messageNew.HasValue)
                {
                    var result = _gameService.Game(messageNew.Value);
                    await botClient.SendTextMessageAsync(playerChoice.Chat.Id,
                        $"Бот выбрал: {RpsItemsExtensions.ToRuLetter(result.BotChoice)}. {GameResultTypesExtensions.ToRuString(result.Type)}.  ", cancellationToken: cancellationToken);
                }
            }
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}