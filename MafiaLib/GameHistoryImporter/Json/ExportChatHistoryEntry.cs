using System.Text.Json.Serialization;

namespace MafiaLib.GameHistoryImporter;

public record ChatTextEntity
{
    public string Type { get; init; }
    public string Text { get; init; }
    public long? UserId { get; init; }
}

public record ExportChatMessage
{
    public long Id { get; init; }

    [JsonPropertyName("date_unixtime")]
    [JsonConverter(typeof(UnixTimeToDateTimeConverter))]
    public DateTime DateUnixTime { get; init; }

    public string Actor { get; init; }
    public string From { get; init; }
    public string FromId { get; init; }

    public IEnumerable<ChatTextEntity> TextEntities { get; init; }
}

public class ExportChatHistoryEntry
{
    public string Name { get; init; }
    public long Id { get; init; }
    public List<ExportChatMessage> Messages { get; init; }
}