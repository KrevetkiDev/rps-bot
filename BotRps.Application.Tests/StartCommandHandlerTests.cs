using BotRpc.Domain.Entities;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Extensions;
using BotRps.Application.Users.Commands.Start;
using BotRps.Tests;
using FluentAssertions;
using NSubstitute;

namespace BotRps.Application.Tests;

public class StartCommandHandlerTests
{
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly StartCommandHandler _startCommandHandler;

    public StartCommandHandlerTests()
    {
        _startCommandHandler = new StartCommandHandler(_repository);
    }

    [Fact]
    public async Task StartCommandHandler_ShouldReturnWhenPlayerAlreadyExistMessage_WhenPlayerAlreadyExist()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Bet = 10, Balance = 10, Nickname = "test" } }
            .AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _startCommandHandler.Handle(new StartCommand() { TelegramId = telegramId }, default);

        // Assert
        result.Text.Should()
            .Be(
                $"Текущая ставка: 10. Для изменения сделай выбор в меню слева\nДелай ход: {RpsItems.Rock.ToEmoji()}, {RpsItems.Scissors.ToEmoji()}, {RpsItems.Paper.ToEmoji()}, {Commands.Balance}?");
    }

    [Fact]
    public async Task StartCommandHandler_ShouldReturnWhenPlayerNullMessage_WhenPlayerNull()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _startCommandHandler.Handle(new StartCommand() { TelegramId = telegramId }, default);

        // Assert
        transactionMock.Received(1).Add(Arg.Is<User>(x =>
            x.Balance == 100 && x.Bet == 10 && x.TelegramId == telegramId && x.Nickname == null));
        result.Text.Should()
            .Be(
                $"Текущая ставка: 10. Для изменения сделай выбор в меню слева\nДелай ход: {RpsItems.Rock.ToEmoji()}, {RpsItems.Scissors.ToEmoji()}, {RpsItems.Paper.ToEmoji()}, {Commands.Balance}?");
    }
}