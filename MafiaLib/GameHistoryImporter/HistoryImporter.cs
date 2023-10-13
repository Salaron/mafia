using System.Text.Json;
using MafiaLib.Database;
using Microsoft.Extensions.Options;

namespace MafiaLib.GameHistoryImporter;

public class HistoryImporter
{
    private readonly AppDbContext _dbContext;
    private readonly JsonHistoryParser _jsonHistoryParser;
    private readonly TextHistoryParser _textHistoryParser;

    public HistoryImporter(AppDbContext dbContext, IOptions<SharedConfig> config)
    {
        _dbContext = dbContext;
        _jsonHistoryParser = new JsonHistoryParser(_dbContext, config);
        _textHistoryParser = new TextHistoryParser(_dbContext);
    }

    public async Task ImportAsync(string chatHistoryJsonString)
    {
        var parsedChatHistory = JsonSerializer.Deserialize<ExportChatHistoryEntry>(chatHistoryJsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        var parseResult = _jsonHistoryParser.Parse(parsedChatHistory);

        await SaveToDatabaseAsync(parseResult);
    }

    public async Task ImportAsync(string messageText, long chatId, DateTime messageTime,
        IEnumerable<MentionedUser> entities)
    {
        var parseResult = _textHistoryParser.Parse(messageText, chatId, messageTime, entities);

        await SaveToDatabaseAsync(parseResult);
    }

    private async Task SaveToDatabaseAsync(HistoryParseResult historyParseResult)
    {
        var newUsers = historyParseResult.Users.Where(us => !_dbContext.Users.Any(u => u.UserId == us.UserId));
        var newGameResults = historyParseResult.GameResults.Where(us =>
            !_dbContext.GameResults.Any(u => u.GameStartDate == us.GameStartDate && u.ChatId == us.ChatId));

        await _dbContext.Users.AddRangeAsync(newUsers);
        await _dbContext.GameResults.AddRangeAsync(newGameResults);
        await _dbContext.SaveChangesAsync();
    }
}