using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Models;
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
            return new Message { Text = "Ты не можешь поставить больше чем у тебя есть!" };
        }

        user.Bet += 10;
        await transaction.CommitAsync(cancellationToken);

        return new Message { Text = $"Текущая ставка: {user.Bet}\nДелай ход!" };
    }
}