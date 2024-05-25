using BotRps.Application.Common;
using BotRps.Application.Users.Commands.Game;
using MediatR;

namespace BotRpc.Bot.Pipes.Base;

public abstract record RpsItemMessagePipeBase(IMediator Mediator) : MessagePipeBase
{
    protected override async Task HandleInternal(PipeContext context, CancellationToken cancellationToken)
    {
        var playerChoice = RpsItemParser.ParseToRps(context.Message);

        if (playerChoice.HasValue)
        {
            var response = await Mediator.Send(new GameCommand() { TelegramId = context.TelegramId, PlayerChoice = playerChoice.Value },
                cancellationToken);

            foreach (var message in response)
            {
                context.ResponseMessages.Add(message);
            }
        }
    }
}