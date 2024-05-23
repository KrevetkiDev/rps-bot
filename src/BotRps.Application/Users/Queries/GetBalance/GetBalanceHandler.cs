using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BotRps.Application.Users.Queries.GetBalance;

public record GetBalanceHandler(IRepository Repository) : IRequestHandler<GetBalanceQuery, Message>
{
    public async Task<Message> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        await using var transaction = await Repository.BeginTransactionAsync<User>(cancellationToken);
        var user = transaction.Set.AsNoTracking().FirstOrDefault(x => x.TelegramId == request.TelegramId);

        return new Message { Text = Messages.BalanceAndBet(user!.Balance, user.Bet) };
    }
}