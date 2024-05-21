using BotRps.Application.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.BetUp;

public class BetUpCommand : IRequest<Message>
{
    public long TelegramId { get; set; }
}