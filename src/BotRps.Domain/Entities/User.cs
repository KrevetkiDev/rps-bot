using BotRpc.Domain.Entities.Base;

namespace BotRpc.Domain.Entities;

public class User : EntityBase
{
    public long TelegramId { get; set; }
    public int Balance { get; set; }
    public int Bet { get; set; }
    public string? Nickname { get; set; }
}