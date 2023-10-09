using MafiaLib.Database;
using MafiaLib.Models;
using Microsoft.Extensions.Options;

namespace MafiaLib.GameHistoryImporter;

internal class JsonHistoryParser(AppDbContext dbContext, IOptions<SharedConfig> config)
{
    private readonly Dictionary<long, TgUser> _userIdsToEntityMap = dbContext.Users.ToDictionary(u => u.UserId, u => u);

    public HistoryParseResult Parse(ExportChatHistoryEntry chatHistory)
    {
        var results = new List<GameResult>();

        var currentGameStartTime = DateTime.MinValue;
        foreach (var message in chatHistory.Messages)
        {
            if (!config.Value.MafiaBotIds.Contains(message.FromId))
                continue;

            if (!message.TextEntities.Any())
                continue;

            var firstTextEntity = message.TextEntities.First();

            if (firstTextEntity.Text == "Игра начинается!")
                currentGameStartTime = message.DateUnixTime;

            if (firstTextEntity.Text == "Игра окончена!")
            {
                var gameResult = ParseGameResult(currentGameStartTime, message);
                gameResult.ChatId = chatHistory.Id;
                results.Add(gameResult);
            }
        }

        return new HistoryParseResult
        {
            GameResults = results,
            Users = _userIdsToEntityMap.Values
        };
    }

    private GameResult ParseGameResult(DateTime gameStartDate, ExportChatMessage chatMessage)
    {
        var gameResult = new GameResult
        {
            ChatId = chatMessage.Id,
            Winners = new List<TgUser>(),
            Roles = new List<GameRole>(),
            IsMafiaWon = false,
            GameStartDate = gameStartDate,
            GameEndDate = chatMessage.DateUnixTime
        };

        using var entityEnumerator = chatMessage.TextEntities.GetEnumerator();

        var isWinnersOver = false;
        while (entityEnumerator.MoveNext())
        {
            var entity = entityEnumerator.Current;
            if (entity.Text.Contains("Победила Мафия"))
                gameResult.IsMafiaWon = true;
            if (entity.Text.Contains("Остальные участники"))
                isWinnersOver = true;

            if (entity.Type == "mention_name")
            {
                if (!_userIdsToEntityMap.ContainsKey(entity.UserId!.Value))
                {
                    var newUser = new TgUser
                    {
                        UserId = entity.UserId!.Value,
                        Name = entity.Text
                    };

                    _userIdsToEntityMap.TryAdd(entity.UserId.Value, newUser);
                }

                var user = _userIdsToEntityMap[entity.UserId.Value];

                entityEnumerator.MoveNext();
                entityEnumerator.MoveNext();
                entity = entityEnumerator.Current;

                var gameRole = new GameRole
                {
                    User = user,
                    Role = entity.Text,
                    GameResult = gameResult
                };
                gameResult.Roles.Add(gameRole);

                if (!isWinnersOver)
                    gameResult.Winners.Add(user);
            }
        }

        return gameResult;
    }
}