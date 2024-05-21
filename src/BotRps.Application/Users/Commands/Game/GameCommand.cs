using BotRpc.Domain.Enums;
using BotRps.Application.Models;
using MediatR;

namespace BotRps.Application.Users.Commands.RpsItem;

public record GameCommand : IRequest<List<Message>>
{
    public long TelegramId { get; set; }

    public RpsItems PlayerChoice { get; set; }
}