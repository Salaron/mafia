using Telegram.Bot;
using Telegram.Bot.Types;

namespace MafiaStatsBot.Bot;

public abstract class BaseCommandHandler : ICommandHandler
{
    public abstract string Command { get; }
    public abstract string HelpText { get; }
    public virtual IEnumerable<BotCommandScope> CommandScopes => new[] { BotCommandScope.Default() };
    public abstract Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken token);
}