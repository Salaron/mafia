using Microsoft.EntityFrameworkCore;

namespace MafiaLib.Models;

[PrimaryKey(nameof(Id))]
public class GameResult
{
    public int Id { get; set; }
    public long ChatId { get; set; }

    public bool IsMafiaWon { get; set; }
    public DateTime GameStartDate { get; set; }
    public DateTime GameEndDate { get; set; }

    public virtual List<TgUser> Winners { get; set; }

    public virtual List<GameRole> Roles { get; set; }
}