using BotRps.Application.Common.Models;
using MediatR;

namespace BotRps.Application.Users.Queries.GetRating;

public class GetRatingQuery : IRequest<Message>
{
    public int TelegramId { get; set; }
}