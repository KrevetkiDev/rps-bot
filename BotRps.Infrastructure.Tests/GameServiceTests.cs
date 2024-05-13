using BotRpc.Domain.Enums;
using BotRps.Infrastructure.Services;
using FluentAssertions;

namespace BotRps.Infrastructure.Tests;

public class GameServiceTests
{
    [Theory]
    [InlineData(RpsItems.Rock, RpsItems.Scissors, GameResultTypes.PlayerWin)]
    [InlineData(RpsItems.Rock, RpsItems.Paper, GameResultTypes.BotWin)]
    [InlineData(RpsItems.Rock, RpsItems.Rock, GameResultTypes.Draw)]
    [InlineData(RpsItems.Scissors, RpsItems.Scissors, GameResultTypes.Draw)]
    [InlineData(RpsItems.Scissors, RpsItems.Paper, GameResultTypes.PlayerWin)]
    [InlineData(RpsItems.Scissors, RpsItems.Rock, GameResultTypes.BotWin)]
    [InlineData(RpsItems.Paper, RpsItems.Paper, GameResultTypes.Draw)]
    [InlineData(RpsItems.Paper, RpsItems.Rock, GameResultTypes.PlayerWin)]
    [InlineData(RpsItems.Paper, RpsItems.Scissors, GameResultTypes.BotWin)]
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