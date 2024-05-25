using BotRpc.Bot.Pipes.Base;
using BotRps.Application.Common;
using BotRps.Application.Users.Commands.BetUp;
using MediatR;

namespace BotRpc.Bot.Pipes;

public record BetUpMessagePipe(IMediator Mediator) : MessagePipeBase
{
    protected override string ApplicableMessage => Commands.BetUpCommand;

    protected override async Task HandleInternal(PipeContext context, CancellationToken cancellationToken)
    {
        var response =
            await Mediator.Send(new BetUpCommand { TelegramId = context.TelegramId }, cancellationToken);

        context.ResponseMessages.Add(response);
    }
}