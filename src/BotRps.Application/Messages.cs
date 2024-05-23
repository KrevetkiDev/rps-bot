using BotRpc.Domain.Entities;
using BotRpc.Domain.Enums;
using BotRps.Application.Common.Extensions;

namespace BotRps.Application;

public class Messages
{
    public const string BetCannotBeLessThanZero = "Ставка не может быть меньше или равна нулю!";

    public static string NotFoundUser = "Ты не найден в бд. Попробуй выполнить команду /start";

    public static string BalanceIsZero =
        "Сейчас тебе не на что играть. Твой баланс скоро обновится и ты сможешь продолжить игру";

    public static string CurrentBet(int bet) => $"Текущая ставка: {bet}\nДелай ход!";

    public static string BetCannotHigherBalance = "Ты не можешь поставить больше чем у тебя есть!";

    public static string BetLowerToBalance(int bet) =>
        $"Твоя ставка установлена до баланса, чтобы ты мог продолжить игру. Текущая ставка: {bet}";

    public static string StartMessage(int bet) =>
        $"Текущая ставка: {bet}. Для изменения сделай выбор в меню слева\nДелай ход: {RpsItems.Rock.ToEmoji()}, {RpsItems.Scissors.ToEmoji()}, {RpsItems.Paper.ToEmoji()}, {Common.Commands.Balance}?";

    public static string BalanceAndBet(int balance, int bet) => $"Твой баланс: {balance}. Твоя ставка {bet}.";

    public static string TopUsers(List<User> topUsers)
    {
        var usersTopList = string.Empty;
        for (var i = 0; i < topUsers.Count; i++)
        {
            var user = topUsers[i];
            var username = user.Nickname == null ? "anon" : $"@{user.Nickname}";
            var userString = $"{i + 1}. {username} - {user.Balance}\n";
            usersTopList += userString;
        }

        return usersTopList;
    }
}