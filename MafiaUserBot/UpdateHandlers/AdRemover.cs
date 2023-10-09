using MafiaUserBot.UserBot;
using TL;
using WTelegram;

namespace MafiaUserBot.UpdateHandlers;

internal class AdRemover : IUpdateHandler
{
    public async Task HandleAsync(Client client, Update update, UserChatsStorage chatsStorage)
    {
        if (update is not UpdateNewMessage { message: Message message })
            return;

        var peer = chatsStorage.GetInputPeer(message.Peer.ID);
        if (message.message.Contains("#РЕКЛАМА"))
            await client.DeleteMessages(peer, message.ID);
    }
}