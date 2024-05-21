using BotRps.Application.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.Start;

public class StartCommand : IRequest<Message>
{
    public long TelegramId { get; set; }
    public string? Username { get; set; }
}