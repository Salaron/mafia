using MafiaLib.Statistic;
using MafiaStatsBot.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
        var chatId = message.Chat.Id;
        if (message.Chat.Type == ChatType.Supergroup)
            chatId = (message.Chat.Id + 1000000000000) * -1;

        var stats = await statsProvider.GetStatsAsync(chatId);

        int i = 0;

        var topTemplate = string.Join('\n', stats.UserTop.Select(x => $"{++i}\t{Math.Round(x.Winrate * 100, 2)}%\t[{x.User.Name}]()tg://user?id={x.User.Id}"));

        var template = $"""
                        Всего игр сыграно: {stats.TotalPlayCount}
                        Из них выйграла мафия: {stats.MafiaWinCount}
                        Среднее время одной игры: {stats.AverageGameDuration:c}

                        Топ (больше 5 побед):
                        {topTemplate}
                        """;

        await botClient.SendTextMessageAsync(message.Chat.Id, template, cancellationToken: token);
    }
}