using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
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
    public async Task StartCommandHandler_ShouldReturnWhenPlayerAlreadyExistsMessage_WhenPlayerAlreadyExists()
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
            .Be(Messages.StartMessage(10));
    }

    [Fact]
    public async Task StartCommandHandler_ShouldReturnWhenPlayerNullMessage_WhenPlayerNull()
    {
        // Arrange
        var telegramId = 1;

        var userMock = new List<User>().AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result =
            await _startCommandHandler.Handle(new StartCommand() { TelegramId = telegramId, Username = "test" },
                default);

        // Assert
        transactionMock.Received(1).Add(Arg.Is<User>(
            x => x.Balance == 100
                 && x.Bet == 10
                 && x.TelegramId == telegramId
                 && x.Nickname == "test"));
        result.Text.Should()
            .Be(Messages.StartMessage(10));
    }
}