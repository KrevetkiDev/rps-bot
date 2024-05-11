using BotRpc.Domain.Enums;
using BotRps.Infrastructure.Services;
using FluentAssertions;

namespace tests;

public class GameServiceTests
{
    [Theory]
    [InlineData(RpsItems.Rock, RpsItems.Scissors, GameResultTypes.PlayerWin)]
    [InlineData(RpsItems.Rock, RpsItems.Paper, GameResultTypes.BotWin)]
    [InlineData(RpsItems.Scissors, RpsItems.Scissors, GameResultTypes.Draw)]
    public void GameService_ShouldReturnResult_ValidArgs(RpsItems playerChoice, RpsItems botChoice,
        GameResultTypes expectedResult)
    {
        // Arrange
        var gameService = new GameService();

        // Act
        var result = gameService.Game(playerChoice, botChoice);

        // Assert
        result.Type.Should().Be(expectedResult);
    }
}