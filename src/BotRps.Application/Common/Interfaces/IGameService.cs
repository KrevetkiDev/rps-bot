using BotRpc.Domain.Enums;
using BotRps.Application.Common.Models;

namespace BotRps.Application.Common.Interfaces;

public interface IGameService
{
    GameResult Game(RpsItems playerChoice, RpsItems botChoice);
    RpsItems GenerateBotChoice();
}