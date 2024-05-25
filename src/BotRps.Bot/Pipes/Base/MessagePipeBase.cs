namespace BotRpc.Bot.Pipes.Base;

public abstract record MessagePipeBase : IMessagePipe
{
    protected abstract string ApplicableMessage { get; }

    public async Task HandleAsync(PipeContext context, CancellationToken cancellationToken)
    {
        if (context.Message == ApplicableMessage)
        {
            await HandleInternal(context, cancellationToken);
        }
    }

    protected abstract Task HandleInternal(PipeContext context, CancellationToken cancellationToken);
}