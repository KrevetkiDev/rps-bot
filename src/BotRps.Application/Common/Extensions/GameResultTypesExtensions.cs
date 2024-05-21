using BotRpc.Domain.Enums;

namespace BotRps.Application.Extensions;

public static class GameResultTypesExtensions
{
    public static string ToRuString(this GameResultTypes item)
    {
        return item switch
        {
            GameResultTypes.PlayerWin => "Ты победил",
            GameResultTypes.BotWin => "Бот победил",
            GameResultTypes.Draw => "Ничья",
            _ => "unknown"
        };
    }
}