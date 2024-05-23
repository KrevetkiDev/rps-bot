using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Users.Queries.GetBalance;
using BotRps.Tests;
using FluentAssertions;
using NSubstitute;

namespace BotRps.Application.Tests;

public class GetBalanceHandlerTests
{
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly GetBalanceHandler _getBalanceHandler;

    public GetBalanceHandlerTests()
    {
        _getBalanceHandler = new GetBalanceHandler(_repository);
    }

    [Fact]
    public async Task GetBalanceHandler_ShouldReturnBalanceMessage_WhenPlayerExist()
    {
        // Arrange
        var telegramId = 1;

        var userMock = new List<User> { new() { TelegramId = telegramId, Bet = 10, Balance = 10 } }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _getBalanceHandler.Handle(new GetBalanceQuery() { TelegramId = telegramId }, default);

        // Assert
        result.Text.Should().Be(Messages.BalanceAndBet(10, 10));
    }
}