namespace BotRpc.Bot.Pipes.Base;

public interface IMessagePipe
{
    Task HandleAsync(PipeContext context, CancellationToken cancellationToken);
}