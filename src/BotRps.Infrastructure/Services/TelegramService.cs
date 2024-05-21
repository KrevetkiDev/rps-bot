using BotRpc.Domain.Enums;
using BotRps.Application;
using BotRps.Application.Extensions;
using BotRps.Application.Interfaces;
using BotRps.Application.Users.Commands.BetDown;
using BotRps.Application.Users.Commands.BetUp;
using BotRps.Application.Users.Commands.RpsItem;
using BotRps.Application.Users.Commands.Start;
using BotRps.Application.Users.Queries.GetBalance;
using BotRps.Application.Users.Queries.GetRating;
using BotRps.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotRps.Infrastructure.Services;

public class TelegramService : ITelegramService, IHostedService
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly ILogger<ITelegramService> _logger;

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

    public TelegramService(IOptions<TelegramOptions> options, IMediator mediator, ILogger<ITelegramService> logger)
    {
        _client = new TelegramBotClient(options.Value.Token);
        _mediator = mediator;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.SetMyCommandsAsync(_commands, cancellationToken: cancellationToken);
        _client.StartReceiving(UpdateHandler, PollingErrorHandler, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;

            if (message != null)
            {
                if (message.Text == "/start")
                {
                    await OnStart(message, cancellationToken);
                }

                if (message.Text == RpsItems.Rock.ToEmoji() ||
                    message.Text == RpsItems.Scissors.ToEmoji() ||
                    message.Text == RpsItems.Paper.ToEmoji())
                {
                    await OnRpsItem(message, cancellationToken);
                }

                if (message.Text == Commands.Balance)
                    await OnBalance(message, cancellationToken);

                if (message.Text == Commands.BetUpCommand)
                    await OnBetUp(message, cancellationToken);

                if (message.Text == Commands.BetDownCommand)
                    await OnBetDown(message, cancellationToken);

                if (message.Text == Commands.Rating)
                    await OnShowRating(message, cancellationToken);
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
        return Task.CompletedTask;
    }

    private async Task OnStart(Message message, CancellationToken cancellationToken)
    {
        if (message.From == null)
            return;

        var response = await _mediator.Send(new StartCommand(), cancellationToken: cancellationToken);

        await _client.SendTextMessageAsync(message.Chat.Id,
            $"{message.Text}",
            replyMarkup: _keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task OnRpsItem(Message message, CancellationToken cancellationToken)
    {
        var playerChoice = RpsItemParser.ParseToRps(message.Text!);

        if (playerChoice.HasValue)
        {
            var response =
                await _mediator.Send(
                    new GameCommand() { TelegramId = message.From!.Id, PlayerChoice = playerChoice.Value },
                    cancellationToken: cancellationToken);
            foreach (var mess in response)
            {
                await _client.SendTextMessageAsync(message.Chat.Id, mess.Text, cancellationToken: cancellationToken);
            }
        }
    }

    private async Task OnBalance(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From!.Id;
        var response = await _mediator.Send(new GetBalanceQuery() { TelegramId = telegramId }, cancellationToken);
        await _client.SendTextMessageAsync(message.Chat.Id, response.Text, cancellationToken: cancellationToken);
    }

    private async Task OnBetUp(Message message, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new BetUpCommand(), cancellationToken);
        await _client.SendTextMessageAsync(message.Chat.Id, response.Text,
            cancellationToken: cancellationToken);
    }

    private async Task OnBetDown(Message message, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new BetDownCommand(), cancellationToken);
        await _client.SendTextMessageAsync(message.Chat.Id, response.Text,
            cancellationToken: cancellationToken);
    }

    private async Task OnShowRating(Message message, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetRatingQuery(), cancellationToken);

        await _client.SendTextMessageAsync(message.Chat.Id, response.Text, cancellationToken: cancellationToken);
    }
}