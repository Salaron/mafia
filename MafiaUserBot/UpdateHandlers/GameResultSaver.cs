using MafiaLib.GameHistoryImporter;
using MafiaUserBot.UserBot;
using Microsoft.Extensions.Options;
using TL;
using WTelegram;

namespace MafiaUserBot.UpdateHandlers;

internal class GameResultSaver(HistoryImporter historyImporter, IOptions<UserBotConfig> config)
    : IUpdateHandler
{
    public async Task HandleAsync(Client client, Update update, UserChatsStorage chatsStorage)
    {
        if (update is not UpdateNewMessage { message: Message message })
            return;

        var isMessageFromMafiaBot = config.Value.MafiaBotIds.Any(id => id.Contains(message.From.ID.ToString())) &&
                                    message.message.Contains("Игра окончена");
        if (!isMessageFromMafiaBot)
            return;

        var peer = chatsStorage.GetInputPeer(message.Peer.ID);
        var channelParticipants = await client.Channels_GetAllParticipants((Channel)chatsStorage.Chats[peer.ID]);

        var roles = message.ToString().Split(Environment.NewLine);

        var mentionedUsers = new List<MentionedUser>();
        foreach (var line in roles)
        {
            if (!line.Contains('-'))
                continue;

            var user = channelParticipants.users.Values.FirstOrDefault(u => line.Contains(u.first_name));
            if (user == null)
                continue;
            mentionedUsers.Add(new MentionedUser
            {
                UserId = user.ID,
                FullName = user.first_name,
                UserName = user.MainUsername
            });
        }

        await historyImporter.ImportAsync(message.message, peer.ID, message.date, mentionedUsers);

        await client.SendMessageAsync(peer, "Я записал \u270d\ufe0f");
    }
}