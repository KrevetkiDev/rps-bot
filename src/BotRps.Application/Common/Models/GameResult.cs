using BotRpc.Domain.Enums;

namespace BotRps.Application.Common.Models;

public class GameResult
{
    public GameResultTypes Type { get; set; }

    public RpsItems BotChoice { get; set; }

    public RpsItems PlayerChoice { get; set; }
}