using BotRpc.Domain.Enums;

namespace BotRps.Application.Extensions;

public class GameResultTypesExtensions
{
    public static string ToRuString(GameResultTypes item)
    {
        return item switch
        {
            GameResultTypes.PlayerWin => "Ты победил",
            GameResultTypes.BotWin => "Бот победил",
            GameResultTypes.Draw => "Ничья",
            _=> "unknown"
        };
    }
}