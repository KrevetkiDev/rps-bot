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

    public TelegramService(IOptions<TelegramOptions> options, IGameService gameService,
        IDbContextFactory<DatabaseContext> dbContextFactory)
    {
        _client = new TelegramBotClient(options.Value.Token);
        _gameService = gameService;
        _dbContextFactory = dbContextFactory;
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
            new KeyboardButton[] { RpsItems.Rock.ToEmoji() },
            new KeyboardButton[] { RpsItems.Scissors.ToEmoji() },
            new KeyboardButton[] { RpsItems.Paper.ToEmoji() }
        })
        {
            ResizeKeyboard = true
        };

        if (playerChoice != null)
        {
            if (playerChoice.Text == "/start")
            {
                var userId = playerChoice.From.Id;
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    if (!dbContext.Users.Any(x => x.TelegramId == userId))
                    {
                        dbContext.Users.Add(new()
                        {
                            Balance = 100,
                            TelegramId = userId
                        });
                        dbContext.SaveChanges();
                    }
                }

                await botClient.SendTextMessageAsync(playerChoice.Chat.Id,
                    $"Делай ход: {RpsItems.Rock.ToEmoji()}, {RpsItems.Scissors.ToEmoji()}, {RpsItems.Paper.ToEmoji()}?",
                    cancellationToken: cancellationToken);
            }

            if (playerChoice.Text == RpsItems.Rock.ToEmoji() || playerChoice.Text == RpsItems.Scissors.ToEmoji() ||
                playerChoice.Text == RpsItems.Paper.ToEmoji())
            {
                var messageNew = RpsItemParser.ParseToRps(playerChoice.Text);
                if (messageNew.HasValue)
                {
                    var result = _gameService.Game(messageNew.Value);
                    await botClient.SendTextMessageAsync(playerChoice.Chat.Id,
                        $"{RpsItemsExtensions.ToEmoji(result.BotChoice)}", cancellationToken: cancellationToken);
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