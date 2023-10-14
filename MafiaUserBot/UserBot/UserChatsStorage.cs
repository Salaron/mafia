using TL;

namespace MafiaUserBot.UserBot;

internal class UserChatsStorage
{
    public Dictionary<long, User> Users { get; } = new();
    public Dictionary<long, ChatBase> Chats { get; } = new();

    public InputPeer GetInputPeer(long peerId)
    {
        return Chats.TryGetValue(peerId, out var peer) ? peer.ToInputPeer() : null;
    }

    public string GetPeerName(Peer peer)
    {
        return peer is null ? null
            : peer is PeerUser user ? GetUser(user.user_id)
            : peer is PeerChat or PeerChannel ? GetChat(peer.ID) : $"Peer {peer.ID}";
    }

    private string GetUser(long id)
    {
        return Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
    }

    private string GetChat(long id)
    {
        return Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
    }
}