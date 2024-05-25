using BotRpc.Domain.Enums;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;

namespace BotRps.Infrastructure.Services;

public class GameService : IGameService
{
    private readonly Random _random = new();

    public GameResult Game(RpsItems playerChoice, RpsItems botChoice)
    {
        var result = new GameResult
        {
            PlayerChoice = playerChoice,
            BotChoice = botChoice
        };

        switch (playerChoice)
        {
            case RpsItems.Rock when botChoice == RpsItems.Paper:
            case RpsItems.Scissors when botChoice == RpsItems.Rock:
            case RpsItems.Paper when botChoice == RpsItems.Scissors:
                result.Type = GameResultTypes.BotWin;
                break;
            case RpsItems.Rock when botChoice == RpsItems.Scissors:
            case RpsItems.Scissors when botChoice == RpsItems.Paper:
            case RpsItems.Paper when botChoice == RpsItems.Rock:
                result.Type = GameResultTypes.PlayerWin;
                break;
            default:
                result.Type = GameResultTypes.Draw;
                break;
        }

        return result;
    }

    public RpsItems GenerateBotChoice()
    {
        var botChoice = (RpsItems)_random.Next(3);
        return botChoice;
    }
}