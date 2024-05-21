using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Users.Queries.GetRating;
using BotRps.Tests;
using FluentAssertions;
using NSubstitute;

namespace BotRps.Application.Tests;

public class GetRatingHandlerTests
{
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly GetRatingHandler _getRatingHandler;

    public GetRatingHandlerTests()
    {
        _getRatingHandler = new GetRatingHandler(_repository);
    }

    [Fact]
    public async Task GetRatingHandler_ShouldReturnRatingMessage_ValidArgs()
    {
        // Arrange
        var telegramId1 = 1;
        var telegramId2 = 2;
        var userMock = new List<User>
        {
            new() { TelegramId = telegramId1, Bet = 10, Balance = 10, Nickname = "test1" },
            new() { TelegramId = telegramId2, Bet = 10, Balance = 10, Nickname = "test2" }
        }.AsEfQueryable();
        var transactionMock = Substitute.For<ITransaction<User>>();
        transactionMock.Set.Returns(userMock);
        _repository.BeginTransactionAsync<User>(default).Returns(Task.FromResult(transactionMock));

        // Act
        var result = await _getRatingHandler.Handle(new GetRatingQuery() { TelegramId = telegramId1 }, default);

        // Assert
        result.Text.Should().Be($"1. @test1 - 10\n2. @test2 - 10\n");
    }
}