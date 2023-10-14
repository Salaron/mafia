using MafiaLib.GameHistoryImporter;
using MafiaUserBot.UserBot;
using Microsoft.Extensions.Options;
using TL;
using WTelegram;

namespace MafiaUserBot.UpdateHandlers;

internal class GameResultSaver : IUpdateHandler
{
    private readonly HistoryImporter _historyImporter;
    private readonly IOptions<UserBotConfig> _config;

    public GameResultSaver(HistoryImporter historyImporter, IOptions<UserBotConfig> config)
    {
        _historyImporter = historyImporter;
        _config = config;
    }

    public async Task HandleAsync(Client client, Update update, UserChatsStorage chatsStorage)
    {
        if (update is not UpdateNewMessage { message: Message message })
            return;

        var isMessageFromMafiaBot = _config.Value.MafiaBotIds.Contains(message.From.ID.ToString()) &&
                                    message.message.Contains("Игра окончена");
        if (!isMessageFromMafiaBot)
            return;

        var peer = chatsStorage.GetInputPeer(message.Peer.ID);

        var chat = await client.GetFullChat(peer);
        var mentionedUsers = chat.users
            .Where(u => message.message.Contains(u.Value.first_name))
            .Select(u => new MentionedUser
            {
                UserId = u.Key,
                FullName = (u.Value.first_name ?? string.Empty) + (u.Value.last_name ?? string.Empty),
                UserName = u.Value.MainUsername
            });

        // in case of group migrated to supergroup
        var groupId = chat.chats.FirstOrDefault(c => c.Value.IsActive && c.Value.IsGroup).Key;
        await _historyImporter.ImportAsync(message.message, groupId, message.date, mentionedUsers);

        await client.SendMessageAsync(peer, "Я записал \u270d\ufe0f");
    }
}