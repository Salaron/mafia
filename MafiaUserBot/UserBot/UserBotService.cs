using Microsoft.Extensions.Options;
using TL;
using WTelegram;

namespace MafiaUserBot.UserBot;

internal class UserBotService(ILogger<UserBotService> logger,
        IServiceProvider serviceProvider,
        IOptions<UserBotConfig> config)
    : BackgroundService
{
    private readonly UserChatsStorage _userChatsStorage = new();
    private Client _client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new Client(Config);

        _client.OnUpdate += OnUpdate;
        await _client.LoginUserIfNeeded();
    }

    private async Task OnUpdate(UpdatesBase updates)
    {
        try
        {
            updates.CollectUsersChats(_userChatsStorage.Users, _userChatsStorage.Chats);

            if (updates is UpdateShortMessage usm && !_userChatsStorage.Users.ContainsKey(usm.user_id))
            {
                var difference = await _client.Updates_GetDifference(usm.pts - usm.pts_count, usm.date, 0);
                difference.CollectUsersChats(_userChatsStorage.Users, _userChatsStorage.Chats);
            }

            if (updates is UpdateShortChatMessage uscm &&
                (!_userChatsStorage.Users.ContainsKey(uscm.from_id) ||
                 !_userChatsStorage.Chats.ContainsKey(uscm.chat_id)))
            {
                var difference = await _client.Updates_GetDifference(uscm.pts - uscm.pts_count, uscm.date, 0);
                difference.CollectUsersChats(_userChatsStorage.Users, _userChatsStorage.Chats);
            }

            using var scope = serviceProvider.CreateScope();
            var updateHandlers = scope.ServiceProvider.GetRequiredService<IEnumerable<IUpdateHandler>>();

            foreach (var update in updates.UpdateList)
            foreach (var updateHandler in updateHandlers)
                await updateHandler.HandleAsync(_client, update, _userChatsStorage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "update error (C91427A7-2B28-42F5-8E2C-5F485FB427DE)");
        }
    }

    private string Config(string what)
    {
        switch (what)
        {
            case "api_id": return config.Value.ApiId;
            case "api_hash": return config.Value.ApiHash;
            case "phone_number": return config.Value.PhoneNumber;
            case "verification_code":
                Console.Write("Code: ");
                return Console.ReadLine();
            case "password": return config.Value.Password;
            default: return null;
        }
    }
}