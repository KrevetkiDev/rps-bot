using BotRpc.Domain.Enums;
using BotRps.Application;
using BotRps.Application.Extensions;
using BotRps.Application.Interfaces;
using BotRps.Infrastructure.Options;
using BotRps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ResetBalanceOptions _resetBalanceOptions;

    private const string Balance = "\ud83d\udcb2";
    private const string BetUpCommand = "/bet_up";
    private const string BetDownCommand = "/bet_down";

    private readonly ReplyKeyboardMarkup _keyboard = new(new[]
    {
        new KeyboardButton[] { RpsItems.Rock.ToEmoji() },
        new KeyboardButton[] { RpsItems.Scissors.ToEmoji() },
        new KeyboardButton[] { RpsItems.Paper.ToEmoji() },
        new KeyboardButton[] { Balance }
    })
    {
        ResizeKeyboard = true
    };

    private readonly List<BotCommand> _commands = new List<BotCommand>()
    {
        new()
        {
            Command = BetUpCommand,
            Description = "Повысить ставку на 10"
        },
        new()
        {
            Command = BetDownCommand,
            Description = "Понизить ставку на 10"
        }
    };

    public TelegramService(IOptions<TelegramOptions> options, IGameService gameService,
        IDbContextFactory<DatabaseContext> dbContextFactory, IOptions<ResetBalanceOptions> resetBalanceOptions)
    {
        _client = new TelegramBotClient(options.Value.Token);
        _gameService = gameService;
        _dbContextFactory = dbContextFactory;
        _resetBalanceOptions = resetBalanceOptions.Value;
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

            if (message.Text == Balance)
            {
                await OnBalance(message, cancellationToken);
            }

            if (message.Text == BetUpCommand)
            {
                await OnBetUp(message, cancellationToken);
            }

            if (message.Text == BetDownCommand)
            {
                await OnBetDown(message, cancellationToken);
            }
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task OnStart(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From.Id;

        using var dbContext = _dbContextFactory.CreateDbContext();
        if (!dbContext.Users.Any(x => x.TelegramId == telegramId))
        {
            dbContext.Users.Add(new()
            {
                Balance = 100,
                TelegramId = telegramId,
                Bet = 10
            });
            dbContext.SaveChanges();
        }

        await _client.SendTextMessageAsync(message.Chat.Id,
            $"Текущая ставка: {dbContext.Users.First(x => x.TelegramId == telegramId).Bet}. Для изменения сделай выбор в меню слева\nДелай ход: {RpsItems.Rock.ToEmoji()}, {RpsItems.Scissors.ToEmoji()}, {RpsItems.Paper.ToEmoji()}, {Balance}?",
            replyMarkup: _keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task OnRpsItem(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From.Id;

        using var dbContext = _dbContextFactory.CreateDbContext();
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);
        if (user.Balance == 0)
        {
            await _client.SendTextMessageAsync(message.Chat.Id,
                $"Тебе не на что играть.  Баланс обновится в {_resetBalanceOptions.ResetTime} ",
                cancellationToken: cancellationToken);
            return;
        }

        if (user.Bet == 0)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, $"Ставка не может быть равна нулю.",
                cancellationToken: cancellationToken);
            return;
        }

        var playerChoice = RpsItemParser.ParseToRps(message.Text);
        if (playerChoice.HasValue)
        {
            var result = _gameService.Game(playerChoice.Value);

            await _client.SendTextMessageAsync(message.Chat.Id,
                $"{RpsItemsExtensions.ToEmoji(result.BotChoice)}",
                cancellationToken: cancellationToken);

            await _client.SendTextMessageAsync(message.Chat.Id,
                $"{GameResultTypesExtensions.ToRuString(result.Type)}",
                cancellationToken: cancellationToken);

            if (result.Type == GameResultTypes.PlayerWin)
            {
                user.Balance += user.Bet;
                if (user.Bet > user.Balance)
                {
                    user.Bet = user.Balance;

                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"Твоя ставка установлена до баланса, чтобы ты мог продолжить игру. Текущая ставка: {user.Bet}",
                        cancellationToken: cancellationToken);
                }

                dbContext.SaveChanges();
            }

            if (result.Type == GameResultTypes.BotWin)
            {
                user.Balance -= user.Bet;

                if (user.Bet > user.Balance && user.Balance > 0)
                {
                    user.Bet = user.Balance;

                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"Твоя ставка установлена до баланса, чтобы ты мог продолжить игру. Текущая ставка: {user.Bet}",
                        cancellationToken: cancellationToken);
                }

                dbContext.SaveChanges();

                if (user.Balance == 0)
                {
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"Твой баланс равен нулю, ты проиграл все деньги. Баланс обновится в {_resetBalanceOptions.ResetTime}",
                        cancellationToken: cancellationToken);
                }
            }
        }
    }

    private async Task OnBalance(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From.Id;

        using var dbContext = _dbContextFactory.CreateDbContext();
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);
        await _client.SendTextMessageAsync(message.Chat.Id, $"Твой баланс: {user.Balance}. Твоя ставка {user.Bet}",
            cancellationToken: cancellationToken);
    }

    private async Task OnBetUp(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From.Id;

        using var dbContext = _dbContextFactory.CreateDbContext();
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);

        if (user.Bet == user.Balance)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, $"Ты не можешь поставить больше чем у тебя есть!",
                cancellationToken: cancellationToken);
            return;
        }

        user.Bet += 10;
        dbContext.SaveChanges();
        await _client.SendTextMessageAsync(message.Chat.Id, $"Текущая ставка: {user.Bet}\nДелай ход!",
            cancellationToken: cancellationToken);
    }

    private async Task OnBetDown(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From.Id;

        using var dbContext = _dbContextFactory.CreateDbContext();
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);

        if (user.Bet <= 10)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, $"Ставка не может быть меньше или равна нулю! ",
                cancellationToken: cancellationToken);
            return;
        }

        user.Bet -= 10;
        dbContext.SaveChanges();
        await _client.SendTextMessageAsync(message.Chat.Id, $"Текущая ставка: {user.Bet}\nДелай ход!",
            cancellationToken: cancellationToken);
    }
}