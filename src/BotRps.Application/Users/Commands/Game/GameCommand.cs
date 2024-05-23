using BotRpc.Domain.Enums;
using BotRps.Application.Common.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.Game;

public record GameCommand : IRequest<List<Message>>
{
    public long TelegramId { get; set; }

    public RpsItems PlayerChoice { get; set; }
}