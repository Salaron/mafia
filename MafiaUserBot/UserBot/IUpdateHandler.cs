using TL;
using WTelegram;

namespace MafiaUserBot.UserBot;

internal interface IUpdateHandler
{
    Task HandleAsync(Client client, Update update, UserChatsStorage chatsStorage);
}