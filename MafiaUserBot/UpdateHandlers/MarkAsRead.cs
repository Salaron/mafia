using MafiaUserBot.UserBot;
using TL;
using WTelegram;

namespace MafiaUserBot.UpdateHandlers;

internal class MarkAsRead : IUpdateHandler
{
    public async Task HandleAsync(Client client, Update update, UserChatsStorage chatsStorage)
    {
        if (update is not UpdateNewMessage { message: Message message })
            return;

        var peer = chatsStorage.GetInputPeer(message.Peer.ID);
        await client.ReadHistory(peer);
    }
}