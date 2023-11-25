using MafiaLib.Database;
using MafiaLib.Models;
using Microsoft.EntityFrameworkCore;

namespace MafiaLib.Statistic;

public class StatsProvider(AppDbContext appDbContext)
{
    public async Task<List<TgUser>> GetUsersAsync(long chatId)
    {
        var users = await appDbContext.GameResults
            .AsNoTracking()
            .Where(result => result.ChatId == chatId)
            .SelectMany(result => result.Roles)
            .Select(result => result.User)
            .Distinct()
            .ToListAsync();

        return users;
    }

    public async Task<List<GameRole>> GetRolesAsync(long chatId, long userId)
    {
        var userRoles = await appDbContext.GameResults
            .AsNoTracking()
            .Include(r => r.Roles)
            .ThenInclude(r => r.User)
            .Include(r => r.Roles)
            .ThenInclude(r => r.GameResult)
            .Where(result => result.ChatId == chatId)
            .OrderByDescending(r => r.GameEndDate)
            .SelectMany(result => result.Roles)
            .Select(result => result)
            .Where(role => role.User.UserId == userId)
            .ToListAsync();

        return userRoles;
    }

    public async Task<StatsByChat> GetStatsAsync(long chatId)
    {
        var gameStats = await appDbContext.GameResults
            .AsNoTracking()
            .Where(result => result.ChatId == chatId)
            .Include(result => result.Winners)
            .Include(result => result.Roles)
            .ToListAsync();

        if (!gameStats.Any())
            return new StatsByChat() { RatingUserTop = new(), WinrateUserTop = new() };

        var averageGameDurationInSeconds = gameStats.Select(s => (s.GameEndDate - s.GameStartDate).TotalSeconds)
            .Average();
        
        // === Compute user top ===
        // Get users and their statistic
        var usersWithStatsFromChat = (await GetUsersAsync(chatId))
            .Select(async x => await GetStatsAsync(chatId, x.UserId))
            .Select(x => x.Result);

        // find maximum of parameters (games, wins, number of roles) for all users
        int maxGames = usersWithStatsFromChat.Max(x => x.PlayCount);
        int maxWins = usersWithStatsFromChat.Max(x => x.WinCount);
        int maxRoles = usersWithStatsFromChat.Max(x => x.GameRoleCountMap.Count);

        var userTop = new List<(TgUser user, int rating)>();
        var winrates = new List<(TgUser user, double winrate)>();

        foreach (var user in usersWithStatsFromChat)
        {
            double kGames = 0.5 * user.PlayCount / maxGames;
            double kWins = (double)user.WinCount / maxWins;
            double kRoles = 0.5 * user.GameRoleCountMap.Count / maxRoles;

            int R = (int)Math.Round(maxGames * kGames * kWins * kRoles * 5);
            
            userTop.Add((user.User, R));
            
            if (user.PlayCount >= 10)
            {
                double wr = Math.Round(100.0 * user.WinCount / user.PlayCount, 2);

                winrates.Add((user.User, wr));
            }
        }

        userTop = userTop.OrderByDescending(x => x.rating).ToList();
        winrates = winrates.OrderByDescending(x => x.winrate).ToList();

        var statsByChat = new StatsByChat
        {
            ChatId = chatId,
            TotalPlayCount = gameStats.Count,
            MafiaWinCount = gameStats.Count(s => s.IsMafiaWon),
            AverageGameDuration = TimeSpan.FromSeconds(averageGameDurationInSeconds),
            RatingUserTop = userTop,
            WinrateUserTop = winrates
        };

        return statsByChat;
    }

    public async Task<StatsByUser> GetStatsAsync(long chatId, long userId)
    {
        var chatUsers = await GetUsersAsync(chatId);

        var tgUser = chatUsers.First(u => u.UserId == userId);

        var playCount = await appDbContext.GameResults
            .AsNoTracking()
            .Include(result => result.Roles)
            .ThenInclude(role => role.User)
            .Include(result => result.Winners)
            .Where(result => result.ChatId == chatId)
            .GroupBy(result => result.ChatId)
            .Select(results => new
            {
                WinCount = results.Count(result => result.Winners.Select(user => user.UserId).Contains(userId)),
                PlayCount = results.Count(result =>
                    result.Roles.Select(user => user.User).Select(r => r.UserId).Contains(userId))
            })
            .FirstAsync();

        var gameRoles = await appDbContext.GameResults
            .AsNoTracking()
            .Include(result => result.Roles)
            .ThenInclude(role => role.User)
            .Where(result => result.ChatId == chatId)
            .SelectMany(result => result.Roles)
            .Where(role => role.User.UserId == userId)
            .GroupBy(r => r.Role)
            .ToDictionaryAsync(r => r.Key, r => r.Count());

        var statsByUser = new StatsByUser
        {
            User = tgUser,
            PlayCount = playCount.PlayCount,
            WinCount = playCount.WinCount,
            GameRoleCountMap = gameRoles
        };

        return statsByUser;
    }
}