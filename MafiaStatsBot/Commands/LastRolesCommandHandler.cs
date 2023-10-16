using MafiaLib.Statistic;
using MafiaStatsBot.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MafiaStatsBot.Commands;

public class LastRolesCommandHandler(StatsProvider statsProvider) : BaseCommandHandler
{
    public override string Command => "last_roles";
    public override string HelpText => "Вывести список ролей из последних игр";

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

        var users = await statsProvider.GetUsersAsync(chatId);

        var userId = message.From.Id;
        // TODO: сделать какой-нибудь нормальный парсер аргументов
        if (message.Text.Length > message.EntityValues.First().Length)
        {
            var userName = message.Text.Substring(message.EntityValues.First().Length + 1);
            var user = users.FirstOrDefault(user => user.Name.Contains(userName));

            if (user != null)
                userId = user.UserId;
        }

        var userRoles = await statsProvider.GetRolesAsync(chatId, userId);
        if (!userRoles.Any())
            return;

        var response = $"[{userRoles.First().User.Name}](tg://user?id={userRoles.First().User.UserId}){Environment.NewLine}";
        foreach (var userRole in userRoles.Take(10))
        {
            var localDate = userRole.GameResult.GameStartDate.ToLocalTime();
            response += $"{userRole.Role} \\- {localDate:d MMMM, HH:mm}{Environment.NewLine}";
        }

        await botClient.SendTextMessageAsync(message.Chat.Id, response, disableNotification: true,
            parseMode: ParseMode.MarkdownV2, cancellationToken: token);
    }
}