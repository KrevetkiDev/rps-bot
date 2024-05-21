using BotRpc.Domain.Entities;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Interfaces;
using BotRps.Application.Extensions;
using BotRps.Application.Models;
using BotRps.Application.Users.Commands.RpsItem;
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
                    Text = "Ты не найден в бд. Попробуй выполнить команду /start"
                }
            ];
        }

        if (user.Balance == 0)
        {
            return
            [
                new Message()
                {
                    Text = "Сейчас тебе не на что играть. Твой баланс скоро обновится и ты сможешь продолжить игру"
                }
            ];
        }

        if (user.Bet == 0)
        {
            return
            [
                new Message()
                {
                    Text = "Ставка не может быть равна нулю"
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
                Text =
                    $"Твоя ставка установлена до баланса, чтобы ты мог продолжить игру. Текущая ставка: {user.Bet}"
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
                    Text = "Сейчас тебе не на что играть. Твой баланс скоро обновится и ты сможешь продолжить игру"
                }
            ];
        }


        return messages;
    }
}