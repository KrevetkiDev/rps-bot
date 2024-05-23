using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Common.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.BetDown;

public record BetDownHandler(IRepository Repository) : IRequestHandler<BetDownCommand, Message>
{
    public async Task<Message> Handle(BetDownCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await Repository.BeginTransactionAsync<User>(cancellationToken);
        var user = transaction.Set.FirstOrDefault(x => x.TelegramId == request.TelegramId);

        if (user!.Bet <= 10)
        {
            return new Message { Text = "Ставка не может быть меньше или равна нулю!" };
        }

        user.Bet -= 10;
        await transaction.CommitAsync(cancellationToken);

        return new Message { Text = $"Текущая ставка: {user.Bet}\nДелай ход!" };
    }
}