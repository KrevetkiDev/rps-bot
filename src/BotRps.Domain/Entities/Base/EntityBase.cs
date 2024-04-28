namespace BotRpc.Domain.Entities.Base;

public class EntityBase
{
    public Guid Id { get; set; }

    protected EntityBase()
    {
    }
}