using BotRpc.Domain.Enums;

namespace BotRps.Application.Common;

public class RpsItemParser
{
    public static RpsItems? ParseToRps(string playerMessage)
    {
        return playerMessage switch
        {
            "\ud83e\udea8" => RpsItems.Rock,
            "\u2702\ufe0f" => RpsItems.Scissors,
            "\ud83d\udcc4" => RpsItems.Paper,
            _ => null
        };
    }
}