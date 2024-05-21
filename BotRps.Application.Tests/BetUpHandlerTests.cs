using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Users.Commands.BetUp;
using BotRps.Tests;
using FluentAssertions;
using NSubstitute;

namespace BotRps.Application.Tests;

public class BetUpHandlerTests
{
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly BetUpHandler _betUpHandler;

    public BetUpHandlerTests()
    {
        _betUpHandler = new BetUpHandler(_repository);
    }

    [Fact]
    public async Task BetUpHandler_ShouldReturnBetHigherThanPermissibleMessage_WhenBetHigherThanPermissible()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Bet = 10, Balance = 10 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _betUpHandler.Handle(new BetUpCommand() { TelegramId = telegramId }, default);

        // Assert
        result.Text.Should().Be("Ты не можешь поставить больше чем у тебя есть!");
    }

    [Fact]
    public async Task BetUpHandler_ShouldReturnBetUpMessage_WhenValidBet()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Bet = 10, Balance = 20 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _betUpHandler.Handle(new BetUpCommand() { TelegramId = telegramId }, default);

        // Assert
        result.Text.Should().Be("Текущая ставка: 20\nДелай ход!");
    }
}