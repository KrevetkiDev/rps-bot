using BotRpc.Bot.Pipes.Base;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Extensions;
using MediatR;

namespace BotRpc.Bot.Pipes;

public record PaperMessagePipe(IMediator Mediator) : RpsItemMessagePipeBase(Mediator)
{
    protected override string ApplicableMessage => RpsItems.Paper.ToEmoji();
}