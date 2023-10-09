using MafiaLib.Models;

namespace MafiaLib.GameHistoryImporter;

public class HistoryParseResult
{
    public IEnumerable<TgUser> Users { get; set; }
    public IEnumerable<GameResult> GameResults { get; set; }
}