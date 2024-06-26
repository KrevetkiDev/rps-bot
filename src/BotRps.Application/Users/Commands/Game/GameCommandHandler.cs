﻿using BotRpc.Domain.Entities;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Extensions;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.Game;

public record GameCommandHandler(IRepository Repository, IGameService GameService)
    : IRequestHandler<GameCommand, List<Message>>
{
    public async Task<List<Message>> Handle(GameCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await Repository.BeginTransactionAsync<User>(cancellationToken);
        var user = transaction.Set.FirstOrDefault(x => x.TelegramId == request.TelegramId);
        var messages = new List<Message>();
        if (user == null)
        {
            return
            [
                new Message()
                {
                    Text = Messages.NotFoundUser
                }
            ];
        }

        if (user.Balance == 0)
        {
            return
            [
                new Message()
                {
                    Text = Messages.BalanceIsZero
                }
            ];
        }

        var botChoice = GameService.GenerateBotChoice();
        var result = GameService.Game(request.PlayerChoice, botChoice);

        var messageBotChoice = new Message()
        {
            Text = result.BotChoice.ToEmoji()
        };

        messages.Add(messageBotChoice);

        if (result.Type == GameResultTypes.PlayerWin)
        {
            user.Balance += user.Bet;
            var messageResult = new Message()
            {
                Text = result.Type.ToRuString()
            };
            messages.Add(messageResult);

            await transaction.CommitAsync(cancellationToken);
        }

        if (result.Type == GameResultTypes.BotWin)
        {
            user.Balance -= user.Bet;
            var messageResult = new Message()
            {
                Text = result.Type.ToRuString()
            };
            messages.Add(messageResult);
            await transaction.CommitAsync(cancellationToken);
        }

        if (result.Type == GameResultTypes.Draw)
        {
            var messageResult = new Message()
            {
                Text = result.Type.ToRuString()
            };
            messages.Add(messageResult);
        }

        if (user.Bet > user.Balance && user.Balance > 0)
        {
            user.Bet = user.Balance;

            var messageBetToBalance = new Message()
            {
                Text = Messages.BetLowerToBalance(user.Bet)
            };

            messages.Add(messageBetToBalance);

            await transaction.CommitAsync(cancellationToken);
        }

        if (user.Balance == 0)
        {
            return
            [
                new Message()
                {
                    Text = Messages.BalanceIsZero
                }
            ];
        }

        return messages;
    }
}