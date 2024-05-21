using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Users.Commands.BetDown;
using BotRps.Tests;
using FluentAssertions;
using NSubstitute;

namespace BotRps.Application.Tests;

public class BetDownHandlerTests
{
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly BetDownHandler _betDownHandler;

    public BetDownHandlerTests()
    {
        _betDownHandler = new BetDownHandler(_repository);
    }

    [Fact]
    public async Task BetDownHandler_ShouldReturnBetLessThanAcceptableMessage_WhenBetLessThanAcceptable()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Bet = 9 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _betDownHandler.Handle(new BetDownCommand() { TelegramId = telegramId }, default);

        // Assert
        result.Text.Should().Be("Ставка не может быть меньше или равна нулю!");
    }

    [Fact]
    public async Task BetDownHandler_ShouldReturnBetDownMessage_WhenValidBet()
    {
        // Arrange
        var telegramId = 1;
        var userMock = new List<User> { new() { TelegramId = telegramId, Bet = 20 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _betDownHandler.Handle(new BetDownCommand() { TelegramId = telegramId }, default);

        // Assert
        result.Text.Should().Be("Текущая ставка: 10\nДелай ход!");
    }
}