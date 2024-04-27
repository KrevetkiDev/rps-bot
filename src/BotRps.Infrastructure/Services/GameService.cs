using BotRpc.Domain.Enums;
using BotRps.Application;
using BotRps.Application.Interfaces;
using BotRps.Application.Models;

namespace BotRps.Infrastructure.Services;

public class GameService : IGameService
{
    public GameResult Game(RpsItems playerChoice)
    {
        Random random = new Random();
        var botChoice = (RpsItems)random.Next(3);
        var result = new GameResult();
        result.BotChoice = botChoice;
        switch (playerChoice == RpsItems.Rock)
        {
            case true when botChoice == RpsItems.Scissors:
                result.Type= GameResultTypes.PlayerWin;
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
}