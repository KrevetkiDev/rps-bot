using BotRpc.Domain.Entities;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Extensions;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;
using BotRps.Application.Users.Commands.Game;
using BotRps.Tests;
using FluentAssertions;
using NSubstitute;

namespace BotRps.Application.Tests;

public class GameCommandHandlerTests
{
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly IGameService _gameService = Substitute.For<IGameService>();
    private readonly GameCommandHandler _gameHandler;

    public GameCommandHandlerTests()
    {
        _gameHandler = new GameCommandHandler(_repository, _gameService);
    }

    [Fact]
    public async Task GameCommandHandler_ShouldReturnUserNotFoundMessage_WhenUserNotFound()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User>().AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _gameHandler.Handle(new GameCommand() { TelegramId = telegramId, PlayerChoice = default },
            default);

        // Assert
        result.First().Text.Should().Be("Ты не найден в бд. Попробуй выполнить команду /start");
    }

    [Fact]
    public async Task GameCommandHandler_ShouldReturnUserBalanceIsZeroMessage_WhenUserBalanceIsZero()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Balance = 0 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _gameHandler.Handle(new GameCommand() { TelegramId = telegramId }, default);

        // Assert
        result.FirstOrDefault()!.Text.Should()
            .Be("Сейчас тебе не на что играть. Твой баланс скоро обновится и ты сможешь продолжить игру");
    }

    [Fact]
    public async Task GameCommandHandler_ShouldReturnPlayerWinMessage_WhenPlayerWin()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Balance = 100, Bet = 10 } }.AsEfQueryable();

        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);

        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));
        _gameService.GenerateBotChoice().Returns(RpsItems.Rock);

        _gameService.Game(RpsItems.Paper, RpsItems.Rock).Returns(new GameResult()
            { PlayerChoice = RpsItems.Paper, BotChoice = RpsItems.Rock, Type = GameResultTypes.PlayerWin });

        // Act
        var result =
            await _gameHandler.Handle(new GameCommand() { TelegramId = telegramId, PlayerChoice = RpsItems.Paper },
                default);

        // Assert
        result.Should().HaveCount(2);
        result[0].Text($"{RpsItems.Rock.ToEmoji()}");
        result[1].Text.Should().Be("Ты победил");
    }

    [Fact]
    public async Task GameCommandHandler_ShouldReturnPlayerLoseMessage_WhenPlayerLose()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Balance = 100, Bet = 10 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));
        _gameService.GenerateBotChoice().Returns(RpsItems.Paper);
        _gameService.Game(RpsItems.Rock, RpsItems.Paper).Returns(new GameResult()
            { PlayerChoice = RpsItems.Rock, BotChoice = RpsItems.Paper, Type = GameResultTypes.BotWin });

        // Act
        var result =
            await _gameHandler.Handle(new GameCommand() { TelegramId = telegramId, PlayerChoice = RpsItems.Rock },
                default);

        // Assert
        result.Count.Should().Be(2);
        result[0].Text.Should().Be($"{RpsItems.Paper.ToEmoji()}");
        result[1].Text.Should().Be("Бот победил");
    }

    [Fact]
    public async Task GameCommandHandler_ShouldReturnDrawMessage_WhenDraw()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Balance = 100, Bet = 10 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));
        _gameService.GenerateBotChoice().Returns(RpsItems.Rock);
        _gameService.Game(RpsItems.Rock, RpsItems.Rock).Returns(new GameResult()
            { PlayerChoice = RpsItems.Rock, BotChoice = RpsItems.Rock, Type = GameResultTypes.Draw });

        // Act
        var result =
            await _gameHandler.Handle(new GameCommand() { TelegramId = telegramId, PlayerChoice = RpsItems.Rock },
                default);

        // Assert
        result.Count.Should().Be(2);
        result[0].Text.Should().Be($"{RpsItems.Rock.ToEmoji()}");
        result[1].Text.Should().Be("Ничья");
    }

    [Fact]
    public async Task GameCommandHandler_ShouldReturnBetGreaterThanBalanceMessage_WhenBetGreaterThanBalance()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Bet = 30, Balance = 40 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));
        _gameService.GenerateBotChoice().Returns(RpsItems.Paper);
        _gameService.Game(RpsItems.Rock, RpsItems.Paper).Returns(new GameResult()
            { PlayerChoice = RpsItems.Rock, BotChoice = RpsItems.Paper, Type = GameResultTypes.BotWin });

        // Act
        var result =
            await _gameHandler.Handle(new GameCommand() { TelegramId = telegramId, PlayerChoice = RpsItems.Rock },
                default);


        // Assert
        result.Count.Should().Be(3);
        result[0].Text.Should().Be($"{RpsItems.Paper.ToEmoji()}");
        result[1].Text.Should().Be("Бот победил");
        result[2].Text.Should()
            .Be($"Твоя ставка установлена до баланса, чтобы ты мог продолжить игру. Текущая ставка: 10");
    }
}