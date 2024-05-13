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
using User = BotRpc.Domain.Entities.User;

namespace BotRps.Infrastructure.Services;

public class TelegramService : ITelegramService, IHostedService
{
    private readonly ITelegramBotClient _client;
    private readonly IGameService _gameService;
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ResetBalanceOptions _resetBalanceOptions;

    private const string Balance = "\ud83d\udcb2";
    private const string Rating = "\ud83c\udfc6";

    private const string BetUpCommand = "/bet_up";
    private const string BetDownCommand = "/bet_down";

    private readonly ReplyKeyboardMarkup _keyboard = new(new KeyboardButton[][]
    {
        [RpsItems.Rock.ToEmoji(), RpsItems.Scissors.ToEmoji(), RpsItems.Paper.ToEmoji()],
        [Balance, Rating]
    })
    {
        ResizeKeyboard = true
    };

    private readonly List<BotCommand> _commands =
    [
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
    ];

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

                if (message.Text == Balance)
                    await OnBalance(message, cancellationToken);

                if (message.Text == BetUpCommand)
                    await OnBetUp(message, cancellationToken);

                if (message.Text == BetDownCommand)
                    await OnBetDown(message, cancellationToken);

                if (message.Text == Rating)
                    await OnShowRating(message, cancellationToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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

        var telegramId = message.From.Id;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);
        if (user == null)
        {
            user = new User
            {
                Balance = 100,
                TelegramId = telegramId,
                Bet = 10,
                Nickname = message.From.Username
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await _client.SendTextMessageAsync(message.Chat.Id,
            $"Текущая ставка: {user.Bet}. Для изменения сделай выбор в меню слева\nДелай ход: {RpsItems.Rock.ToEmoji()}, {RpsItems.Scissors.ToEmoji()}, {RpsItems.Paper.ToEmoji()}, {Balance}?",
            replyMarkup: _keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task OnRpsItem(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From!.Id;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);
        if (user == null)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "Ты не найден в бд. Попробуй выполнить команду /start.",
                cancellationToken: cancellationToken);
            return;
        }

        if (user.Balance == 0)
        {
            await _client.SendTextMessageAsync(message.Chat.Id,
                $"Тебе не на что играть. Баланс обновится в {_resetBalanceOptions.ResetTime}",
                cancellationToken: cancellationToken);
            return;
        }

        if (user.Bet == 0)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, $"Ставка не может быть равна нулю.",
                cancellationToken: cancellationToken);
            return;
        }

        var playerChoice = RpsItemParser.ParseToRps(message.Text!);
        if (playerChoice.HasValue)
        {
            var botChoice = _gameService.GenerateBotChoice();
            var result = _gameService.Game(playerChoice.Value, botChoice);

            if (result.Type == GameResultTypes.PlayerWin)
            {
                user.Balance += user.Bet;

                await dbContext.SaveChangesAsync(cancellationToken);
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

                await dbContext.SaveChangesAsync(cancellationToken);

                if (user.Balance == 0)
                {
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"Твой баланс равен нулю, ты проиграл все деньги. Баланс обновится в {_resetBalanceOptions.ResetTime}",
                        cancellationToken: cancellationToken);
                }
            }

            await _client.SendTextMessageAsync(message.Chat.Id, $"{result.BotChoice.ToEmoji()}",
                cancellationToken: cancellationToken);

            await _client.SendTextMessageAsync(message.Chat.Id, $"{result.Type.ToRuString()}",
                cancellationToken: cancellationToken);
        }
    }

    private async Task OnBalance(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From!.Id;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = dbContext.Users.AsNoTracking().FirstOrDefault(x => x.TelegramId == telegramId);
        await _client.SendTextMessageAsync(message.Chat.Id, $"Твой баланс: {user!.Balance}. Твоя ставка {user.Bet}",
            cancellationToken: cancellationToken);
    }

    private async Task OnBetUp(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From!.Id;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);

        if (user!.Bet == user.Balance)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, $"Ты не можешь поставить больше чем у тебя есть!",
                cancellationToken: cancellationToken);
            return;
        }

        user.Bet += 10;
        await dbContext.SaveChangesAsync(cancellationToken);
        await _client.SendTextMessageAsync(message.Chat.Id, $"Текущая ставка: {user.Bet}\nДелай ход!",
            cancellationToken: cancellationToken);
    }

    private async Task OnBetDown(Message message, CancellationToken cancellationToken)
    {
        var telegramId = message.From!.Id;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = dbContext.Users.FirstOrDefault(x => x.TelegramId == telegramId);

        if (user!.Bet <= 10)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, $"Ставка не может быть меньше или равна нулю! ",
                cancellationToken: cancellationToken);
            return;
        }

        user.Bet -= 10;
        await dbContext.SaveChangesAsync(cancellationToken);
        await _client.SendTextMessageAsync(message.Chat.Id, $"Текущая ставка: {user.Bet}\nДелай ход!",
            cancellationToken: cancellationToken);
    }

    private async Task OnShowRating(Message message, CancellationToken cancellationToken)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var topUsers = dbContext.Users.AsNoTracking().OrderByDescending(x => x.Balance).Take(10).ToList();
        var usersTopList = string.Empty;
        for (int i = 0; i < topUsers.Count; i++)
        {
            var user = topUsers[i];
            var username = user.Nickname == null ? "anon" : $"@{user.Nickname}";
            var userString = $"{i + 1}. {username} - {user.Balance}\n";
            usersTopList += userString;
        }

        await _client.SendTextMessageAsync(message.Chat.Id, usersTopList, cancellationToken: cancellationToken);
    }
}