using BotRps.Application.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.BetDown;

public record BetDownCommand : IRequest<Message>
{
    public long TelegramId { get; set; }
}