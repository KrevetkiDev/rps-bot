using BotRps.Application.Common.Models;
using MediatR;

namespace BotRps.Application.Users.Queries.GetBalance;

public class GetBalanceQuery : IRequest<Message>
{
    public long TelegramId { get; set; }
}