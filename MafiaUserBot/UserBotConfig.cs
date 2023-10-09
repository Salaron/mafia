using MafiaLib;

namespace MafiaUserBot;

public class UserBotConfig : SharedConfig
{
    public string ApiId { get; init; }
    public string ApiHash { get; init; }
    public string PhoneNumber { get; init; }
    public string Password { get; init; }
}