using MafiaLib.Models;

namespace MafiaLib.Statistic;

public class StatsByChat
{
    public long ChatId { get; set; }
    public int TotalPlayCount { get; set; }
    public int MafiaWinCount { get; set; }
    public TimeSpan AverageGameDuration { get; set; }
    public List<(TgUser user, int rating)> UserTop { get; set; }
}