using BotRpc.Domain.Entities;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Extensions;
using BotRps.Application.Models;
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
            Text =
                $"Текущая ставка: {user.Bet}. Для изменения сделай выбор в меню слева\nДелай ход: {RpsItems.Rock.ToEmoji()}, {RpsItems.Scissors.ToEmoji()}, {RpsItems.Paper.ToEmoji()}, {Application.Commands.Balance}?"
        };
    }
}