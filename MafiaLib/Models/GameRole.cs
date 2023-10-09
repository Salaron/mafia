using Microsoft.EntityFrameworkCore;

namespace MafiaLib.Models;

[PrimaryKey(nameof(Id))]
public class GameRole
{
    public int Id { get; set; }
    public int GameResultId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; }
    
    public GameResult GameResult { get; set; }
    public TgUser User { get; set; }
}