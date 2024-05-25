using BotRpc.Bot.Pipes.Base;
using BotRps.Application.Common;
using BotRps.Application.Users.Queries.GetBalance;
using MediatR;

namespace BotRpc.Bot.Pipes;

public record BalanceMessagePipe(IMediator Mediator) : MessagePipeBase
{
    protected override string ApplicableMessage => Commands.Balance;

    protected override async Task HandleInternal(PipeContext context, CancellationToken cancellationToken)
    {
        var response =
            await Mediator.Send(new GetBalanceQuery { TelegramId = context.TelegramId }, cancellationToken);

        context.ResponseMessages.Add(response);
    }
}