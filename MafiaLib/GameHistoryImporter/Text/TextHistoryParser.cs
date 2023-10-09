using System.Text.RegularExpressions;
using MafiaLib.Database;
using MafiaLib.Models;
using Microsoft.Extensions.Options;

namespace MafiaLib.GameHistoryImporter;

public partial class TextHistoryParser(AppDbContext dbContext)
{
    private readonly Dictionary<long, TgUser> _userIdsToEntityMap = dbContext.Users.ToDictionary(u => u.UserId, u => u);
    private int _currentRoleIdx;

    public HistoryParseResult Parse(string messageText, long chatId, DateTime messageTime,
        IEnumerable<MessageEntity> entities)
    {
        var endGameText = messageText.Split(Environment.NewLine).Last();
        var gameDuration = ParseGameDuration(endGameText);

        var gameResult = new GameResult
        {
            ChatId = chatId,
            Winners = new List<TgUser>(),
            Roles = new List<GameRole>(),
            IsMafiaWon = false,
            GameStartDate = messageTime - gameDuration,
            GameEndDate = messageTime
        };

        var isWinnersOver = false;
        foreach (var line in messageText.Split(Environment.NewLine))
        {
            if (line.Contains("Победила Мафия"))
                gameResult.IsMafiaWon = true;
            if (line.Contains("Остальные участники"))
                isWinnersOver = true;

            if (!line.Contains('-'))
                continue;

            var gameRole = ParseRole(line, entities);
            if (gameRole == null)
                continue;

            gameResult.Roles.Add(gameRole);
            if (!isWinnersOver)
                gameResult.Winners.Add(gameRole.User);
        }

        var historyParseResult = new HistoryParseResult
        {
            Users = _userIdsToEntityMap.Values,
            GameResults = new[] { gameResult }
        };

        return historyParseResult;
    }

    private GameRole ParseRole(string line, IEnumerable<MessageEntity> entities)
    {
        var (name, role) = line.Split("-").Select(v => v.Trim());

        var user = default(TgUser);
        var mentionEntity = entities.Where(e => e.Type == "mention").Skip(_currentRoleIdx).FirstOrDefault();
        if (mentionEntity == null)
        {
            var userFromDb = dbContext.Users.FirstOrDefault(u => u.Name.Contains(name));
            if (userFromDb == null)
                return null;

            user = userFromDb;
        }
        else if (!_userIdsToEntityMap.ContainsKey(mentionEntity.UserId))
        {
            var newUser = new TgUser
            {
                UserId = mentionEntity.UserId,
                Name = name
            };
            _userIdsToEntityMap.TryAdd(mentionEntity.UserId, newUser);
            user = newUser;
        }

        _currentRoleIdx++;

        var gameRole = new GameRole
        {
            User = user,
            Role = role
        };

        return gameRole;
    }

    private TimeSpan ParseGameDuration(string endGameText)
    {
        var minutesRegex = MinutesRegex().Match(endGameText);
        var secondsRegex = SecondsRegex().Match(endGameText);

        var gameDurationInSeconds = 0;
        if (minutesRegex.Success)
            gameDurationInSeconds += int.Parse(minutesRegex.Value) * 60;
        if (secondsRegex.Success)
            gameDurationInSeconds += int.Parse(secondsRegex.Value);

        return TimeSpan.FromSeconds(gameDurationInSeconds);
    }

    [GeneratedRegex(@"(\d+)\s*мин\.")]
    private static partial Regex MinutesRegex();

    [GeneratedRegex(@"(\d+)\s*сек\.")]
    private static partial Regex SecondsRegex();
}