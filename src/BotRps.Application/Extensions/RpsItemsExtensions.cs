using BotRpc.Domain.Enums;

namespace BotRps.Application.Extensions;

public class RpsItemsExtensions
{
    public static string ToRuLetter(RpsItems item)
    {
        return item switch
        {
            RpsItems.Rock => "к",
            RpsItems.Scissors => "н",
            RpsItems.Paper => "б",
            _=> "unknown"
        };
    }
}