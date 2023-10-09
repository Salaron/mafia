using MafiaLib.Models;

namespace MafiaLib.Statistic;

public class StatsByUser
{
    public TgUser User { get; set; }
    public int PlayCount { get; set; }
    public int WinCount { get; set; }
    public Dictionary<string, int> GameRoleCountMap { get; set; }
}