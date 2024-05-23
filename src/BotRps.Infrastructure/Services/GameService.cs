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
        switch (playerChoice == RpsItems.Rock)
        {
            case true when botChoice == RpsItems.Scissors:
                result.Type = GameResultTypes.PlayerWin;
                break;

            case true when botChoice == RpsItems.Paper:
                result.Type = GameResultTypes.BotWin;
                break;
        }

        switch (playerChoice == RpsItems.Scissors)
        {
            case true when botChoice == RpsItems.Rock:
                result.Type = GameResultTypes.BotWin;
                break;

            case true when botChoice == RpsItems.Paper:
                result.Type = GameResultTypes.PlayerWin;
                break;
        }

        switch (playerChoice == RpsItems.Paper)
        {
            case true when botChoice == RpsItems.Rock:
                result.Type = GameResultTypes.PlayerWin;
                break;

            case true when botChoice == RpsItems.Scissors:
                result.Type = GameResultTypes.BotWin;
                break;
        }

        if (playerChoice == botChoice)
        {
            result.Type = GameResultTypes.Draw;
        }

        return result;
    }

    public RpsItems GenerateBotChoice()
    {
        var botChoice = (RpsItems)_random.Next(3);
        return botChoice;
    }
}