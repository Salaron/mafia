using MafiaLib.Statistic;
using MafiaStatsBot.Bot;
using Npgsql;
using System.Text;
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

        var topTemplate = new StringBuilder();

        for (int i = 0; i < stats.UserTop.Count; i++)
        {
            var stat = stats.UserTop[i];

            topTemplate.AppendLine($"{i + 1}. [{stat.user.Name}](tg://user?id={stat.user.UserId}) {stat.rating}");
        }

        var template = $"""
                        Всего игр сыграно: {stats.TotalPlayCount}
                        Из них выиграла мафия: {stats.MafiaWinCount}
                        Среднее время одной игры: {stats.AverageGameDuration:c}

                        Топ (больше 5 побед):
                        {topTemplate}
                        """;

        await botClient.SendTextMessageAsync(message.Chat.Id, template, disableNotification: false, parseMode: ParseMode.MarkdownV2, cancellationToken: token);
    }
}