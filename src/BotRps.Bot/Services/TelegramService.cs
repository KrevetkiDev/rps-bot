using BotRpc.Bot.Pipes;
using BotRpc.Bot.Pipes.Base;
using BotRpc.Domain.Enums;
using BotRps.Application.Common;
using BotRps.Application.Common.Extensions;
using BotRps.Application.Common.Interfaces;
using BotRps.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotRpc.Bot.Services;

public class TelegramService : ITelegramService, IHostedService
{
    private readonly ITelegramBotClient _client;
    private readonly ILogger<ITelegramService> _logger;
    private readonly IEnumerable<IMessagePipe> _messagePipes;

    private readonly ReplyKeyboardMarkup _keyboard = new(new KeyboardButton[][]
    {
        [RpsItems.Rock.ToEmoji(), RpsItems.Scissors.ToEmoji(), RpsItems.Paper.ToEmoji()],
        [Commands.Balance, Commands.Rating]
    })
    {
        ResizeKeyboard = true
    };

    private readonly List<BotCommand> _commands =
    [
        new()
        {
            Command = Commands.BetUpCommand,
            Description = "Повысить ставку на 10"
        },

        new()
        {
            Command = Commands.BetDownCommand,
            Description = "Понизить ставку на 10"
        }
    ];

    public TelegramService(IOptions<TelegramOptions> options, ILogger<ITelegramService> logger,
        IEnumerable<IMessagePipe> messagePipes)
    {
        _client = new TelegramBotClient(options.Value.Token);
        _logger = logger;
        _messagePipes = messagePipes;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.SetMyCommandsAsync(_commands, cancellationToken: cancellationToken);
        _client.StartReceiving(UpdateHandler, PollingErrorHandler, cancellationToken: cancellationToken);

        _logger.LogInformation("Telegram Service started");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telegram Service stopped");
        return Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received message {Message}", update.Message);

            if (update.Message != null && update.Message.Text != null)
            {
                var context = new PipeContext
                {
                    TelegramId = update.Message.From!.Id,
                    Message = update.Message.Text!,
                    Username = update.Message.From.Username
                };

                foreach (var pipe in _messagePipes)
                {
                    await pipe.HandleAsync(context, cancellationToken);
                }

                foreach (var message in context.ResponseMessages)
                {
                    await _client.SendTextMessageAsync(update.Message.Chat.Id, message.Text, replyMarkup: _keyboard,
                        cancellationToken: cancellationToken);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error handling message");
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram client error");
        return Task.CompletedTask;
    }
}