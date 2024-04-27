using BotRpc.Domain.Enums;

namespace BotRps.Application.Extensions;

public class RpsItemsExtensions
{
    public static string ToButton(RpsItems item)
    {
        return item switch
        {
            RpsItems.Rock => "\ud83e\udea8",
            RpsItems.Scissors => "\u2702\ufe0f",
            RpsItems.Paper => "\ud83d\udcc4",
            _ => "unknown"
        };
    }
}