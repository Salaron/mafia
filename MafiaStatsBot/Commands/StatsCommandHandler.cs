using MafiaLib.Statistic;
using MafiaStatsBot.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MafiaStatsBot.Commands;

public class StatsCommandHandler(StatsProvider statsProvider) : BaseCommandHandler
{
    public override string Command => "stats";
    public override string HelpText => "Показать статистику по игроку";

    public override IEnumerable<BotCommandScope> CommandScopes => new BotCommandScope[]
    {
        BotCommandScope.AllGroupChats(),
        BotCommandScope.AllChatAdministrators()
    };

    public override async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken token)
    {
        var users = await statsProvider.GetUsersAsync(message.Chat.Id);
        var userId = message.From.Id;

        if (message.Text.Length > message.EntityValues.First().Length)
        {
            var userName = message.Text.Substring(message.EntityValues.First().Length + 1);
            var user = users.FirstOrDefault(user => user.Name.Contains(userName));
        
            if (user != null)
                userId = user.UserId;
        }

        var userStats = await statsProvider.GetStatsAsync(message.Chat.Id, userId);
        var response = $"""
                        [{userStats.User.Name}](tg://user?id={userStats.User.UserId})

                        Игр сыграно: {userStats.PlayCount}
                        Из них победных: {userStats.WinCount}

                        Роли:

                        """;

        foreach (var (role, count) in userStats.GameRoleCountMap.OrderBy(x => x.Value))
        {
            response += $"{role} \\- {count}{Environment.NewLine}";
        }

        await botClient.SendTextMessageAsync(message.Chat.Id, response, disableNotification: true,
            parseMode: ParseMode.MarkdownV2, cancellationToken: token);
    }
}