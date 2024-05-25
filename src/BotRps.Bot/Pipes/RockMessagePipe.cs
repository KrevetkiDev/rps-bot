using BotRpc.Bot.Pipes.Base;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Extensions;
using MediatR;

namespace BotRpc.Bot.Pipes;

public record RockMessagePipe(IMediator Mediator) : RpsItemMessagePipeBase(Mediator)
{
    protected override string ApplicableMessage => RpsItems.Rock.ToEmoji();
}