using Microsoft.EntityFrameworkCore;

namespace MafiaLib.Models;

[PrimaryKey(nameof(Id))]
public class TgUser
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; }

    public virtual IEnumerable<GameResult> WinGames { get; set; }
}