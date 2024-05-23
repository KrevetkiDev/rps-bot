using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BotRps.Application.Users.Queries.GetRating;

public record GetRatingHandler(IRepository Repository) : IRequestHandler<GetRatingQuery, Message>
{
    public async Task<Message> Handle(GetRatingQuery request, CancellationToken cancellationToken)
    {
        await using var transaction = await Repository.BeginTransactionAsync<User>(cancellationToken);
        var topUsers = transaction.Set.AsNoTracking().OrderByDescending(x => x.Balance).Take(10).ToList();
        var usersTopList = Messages.TopUsers(topUsers);

        return new Message()
        {
            Text = usersTopList
        };
    }
}