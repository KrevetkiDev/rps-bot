using BotRpc.Bot.Pipes.Base;
using BotRps.Application.Common;
using BotRps.Application.Users.Queries.GetRating;
using MediatR;

namespace BotRpc.Bot.Pipes;

public record ShowRatingMessagePipe(IMediator Mediator) : MessagePipeBase
{
    protected override string ApplicableMessage => Commands.Rating;

    protected override async Task HandleInternal(PipeContext context, CancellationToken cancellationToken)
    {
        var response =
            await Mediator.Send(new GetRatingQuery(), cancellationToken);

        context.ResponseMessages.Add(response);
    }
}