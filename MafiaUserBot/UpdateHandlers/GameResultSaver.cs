using MafiaLib.GameHistoryImporter;
using MafiaUserBot.UserBot;
using Microsoft.Extensions.Options;
using TL;
using WTelegram;

namespace MafiaUserBot.UpdateHandlers;

internal class GameResultSaver(HistoryImporter historyImporter, IOptions<UserBotConfig> config) : IUpdateHandler
{
    public async Task HandleAsync(Client client, Update update, UserChatsStorage chatsStorage)
    {
        if (update is not UpdateNewMessage { message: Message message })
            return;

        var isMessageFromMafiaBot = config.Value.MafiaBotIds.Contains(message.From.ID.ToString()) &&
                                    message.message.Contains("Игра окончена");
        if (isMessageFromMafiaBot)
            return;

        var entities = message.entities.Select(entity => new MafiaLib.GameHistoryImporter.MessageEntity
        {
            Type = entity.Type,
            // TODO: проверить типы, возможно есть ещё какой-то mention, из-за которого не все юзеры парсятся
            UserId = entity is MessageEntityMentionName mention ? mention.user_id : 0
        });

        await historyImporter.ImportAsync(message.message, message.Peer.ID, message.date, entities);

        var peer = chatsStorage.GetInputPeer(message.Peer.ID);
        await client.SendMessageAsync(peer, "Я записал \u270d\ufe0f");
    }
}