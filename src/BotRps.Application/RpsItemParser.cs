using BotRpc.Domain.Enums;

namespace BotRps.Application;

public class RpsItemParser
{
    public static RpsItems? ParseToRps(string playerMessage)
    {
        return playerMessage switch
        {
            "к" => RpsItems.Rock,
            "н" => RpsItems.Scissors,
            "б" => RpsItems.Paper,
            _ => null
        };
    }
}