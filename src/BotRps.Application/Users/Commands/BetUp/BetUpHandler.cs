﻿using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.BetUp;

public record BetUpHandler(IRepository Repository) : IRequestHandler<BetUpCommand, Message>
{
    public async Task<Message> Handle(BetUpCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await Repository.BeginTransactionAsync<User>(cancellationToken);
        var user = transaction.Set.FirstOrDefault(x => x.TelegramId == request.TelegramId);

        if (user!.Bet == user.Balance)
        {
            return new Message { Text = Messages.BetCannotHigherBalance };
        }

        user.Bet += 10;
        await transaction.CommitAsync(cancellationToken);

        return new Message { Text = Messages.CurrentBet(user.Bet) };
    }
}