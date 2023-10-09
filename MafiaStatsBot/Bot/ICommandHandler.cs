using Telegram.Bot;
using Telegram.Bot.Types;

namespace MafiaStatsBot.Bot;

public interface ICommandHandler
{
    Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken token);
}