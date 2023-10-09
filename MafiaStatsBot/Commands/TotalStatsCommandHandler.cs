using MafiaLib.Statistic;
using MafiaStatsBot.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MafiaStatsBot.Commands;

public class TotalStatsCommandHandler(StatsProvider statsProvider) : BaseCommandHandler
{
    public override string Command => "total";
    public override string HelpText => "Показать статистику по чату";

    public override IEnumerable<BotCommandScope> CommandScopes => new BotCommandScope[]
    {
        BotCommandScope.AllGroupChats(),
        BotCommandScope.AllChatAdministrators()
    };

    public override async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken token)
    {
        var stats = await statsProvider.GetStatsAsync(message.Chat.Id);

        var template = $"""
                        Всего игр сыграно: {stats.TotalPlayCount}
                        Из них выйграла мафия: {stats.MafiaWinCount}
                        Среднее время одной игры: {stats.AverageGameDuration:c}
                        
                        Топ крутых:
                        """;

        await botClient.SendTextMessageAsync(message.Chat.Id, template, cancellationToken: token);
    }
}