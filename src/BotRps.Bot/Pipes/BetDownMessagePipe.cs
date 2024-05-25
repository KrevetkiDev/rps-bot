using BotRpc.Bot.Pipes.Base;
using BotRps.Application.Common;
using BotRps.Application.Users.Commands.BetDown;
using MediatR;

namespace BotRpc.Bot.Pipes;

public record BetDownMessagePipe(IMediator Mediator) : MessagePipeBase
{
    protected override string ApplicableMessage => Commands.BetDownCommand;

    protected override async Task HandleInternal(PipeContext context, CancellationToken cancellationToken)
    {
        var response =
            await Mediator.Send(new BetDownCommand { TelegramId = context.TelegramId }, cancellationToken);

        context.ResponseMessages.Add(response);
    }
}