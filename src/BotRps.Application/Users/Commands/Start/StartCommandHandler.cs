using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BotRps.Application.Users.Commands.Start;

public record StartCommandHandler(IRepository Repository) : IRequestHandler<StartCommand, Message>
{
    public async Task<Message> Handle(StartCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await Repository.BeginTransactionAsync<User>(cancellationToken);

        var user = transaction.Set.AsNoTracking().FirstOrDefault(x => x.TelegramId == request.TelegramId);
        if (user == null)
        {
            user = new User
            {
                Balance = 100,
                TelegramId = request.TelegramId,
                Bet = 10,
                Nickname = request.Username
            };
            transaction.Add(user);
            await transaction.CommitAsync(cancellationToken);
        }

        return new Message
        {
            Text = Messages.StartMessage(user.Bet)
        };
    }
}