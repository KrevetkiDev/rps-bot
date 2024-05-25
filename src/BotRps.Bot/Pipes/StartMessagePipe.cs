using BotRpc.Bot.Pipes.Base;
using BotRps.Application.Users.Commands.Start;
using MediatR;

namespace BotRpc.Bot.Pipes;

public record StartMessagePipe(IMediator Mediator) : MessagePipeBase
{
    protected override string ApplicableMessage => "/start";

    protected override async Task HandleInternal(PipeContext context, CancellationToken cancellationToken)
    {
        var response =
            await Mediator.Send(new StartCommand { TelegramId = context.TelegramId, Username = context.Username }, cancellationToken);

        context.ResponseMessages.Add(response);
    }
}