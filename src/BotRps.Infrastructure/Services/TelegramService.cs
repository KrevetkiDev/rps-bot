using BotRps.Application;
using BotRps.Application.Extensions;
using BotRps.Application.Interfaces;
using BotRps.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "\ud83e\udea8" },
            new KeyboardButton[] { "\u2702\ufe0f" },
            new KeyboardButton[] { "\ud83d\udcc4" }
        })
        {
            ResizeKeyboard = true
        };

        if (playerChoice != null)
        {
            if (playerChoice.Text == "/start")
            {
                await botClient.SendTextMessageAsync(playerChoice.Chat.Id,
                    "Делай ход: \ud83e\udea8, \u2702\ufe0f, \ud83d\udcc4?", cancellationToken: cancellationToken);
            }

            if (playerChoice.Text == "\ud83e\udea8" || playerChoice.Text == "\u2702\ufe0f" ||
                playerChoice.Text == "\ud83d\udcc4")
            {
                var messageNew = RpsItemParser.ParseToRps(playerChoice.Text);
                if (messageNew.HasValue)
                {
                    var result = _gameService.Game(messageNew.Value);
                    await botClient.SendTextMessageAsync(playerChoice.Chat.Id,
                        $"{RpsItemsExtensions.ToButton(result.BotChoice)}", cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(playerChoice.Chat.Id,
                        $"{GameResultTypesExtensions.ToRuString(result.Type)}", cancellationToken: cancellationToken);
                }
            }
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}