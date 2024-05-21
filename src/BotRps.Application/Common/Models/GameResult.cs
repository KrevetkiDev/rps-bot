using BotRpc.Domain.Enums;

namespace BotRps.Application.Models;

public class GameResult
{
    public GameResultTypes Type { get; set; }

    public RpsItems BotChoice { get; set; }

    public RpsItems PlayerChoice { get; set; }
}