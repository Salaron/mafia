using TL;

namespace MafiaUserBot.UserBot;

internal class UserChatsStorage
{
    public Dictionary<long, User> Users { get; } = new();
    public Dictionary<long, ChatBase> Chats { get; } = new();

    public InputPeer GetInputPeer(long peerId) => Chats.TryGetValue(peerId, out var peer) ? peer.ToInputPeer() : null;

    public string GetPeerName(Peer peer) => peer is null ? null
        : peer is PeerUser user ? GetUser(user.user_id)
        : peer is PeerChat or PeerChannel ? GetChat(peer.ID) : $"Peer {peer.ID}";

    private string GetUser(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
    private string GetChat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
}