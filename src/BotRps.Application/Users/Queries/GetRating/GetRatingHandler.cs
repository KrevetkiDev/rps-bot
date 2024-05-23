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
        var usersTopList = string.Empty;
        for (int i = 0; i < topUsers.Count; i++)
        {
            var user = topUsers[i];
            var username = user.Nickname == null ? "anon" : $"@{user.Nickname}";
            var userString = $"{i + 1}. {username} - {user.Balance}\n";
            usersTopList += userString;
        }

        return new Message()
        {
            Text = usersTopList
        };
    }
}