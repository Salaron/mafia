using MafiaLib.GameHistoryImporter;
using MafiaStatsBot.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MafiaStatsBot.Commands;

public class ImportCommandHandler(HistoryImporter historyImporter) : BaseCommandHandler
{
    public override string Command => "import";
    public override string HelpText => "Импорт истории чата";

    public override IEnumerable<BotCommandScope> CommandScopes => new BotCommandScope[]
    {
        BotCommandScope.AllPrivateChats()
    };

    public override async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken token)
    {
        if (message.Document is null)
            return;

        var file = await botClient.GetFileAsync(message.Document.FileId, token);

        await using var ms = new MemoryStream();
        await botClient.DownloadFileAsync(file.FilePath, ms, token);

        ms.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(ms);
        var chatHistoryJsonString = await streamReader.ReadToEndAsync(token);

        // TODO: json validation
        await historyImporter.ImportAsync(chatHistoryJsonString);

        await botClient.SendTextMessageAsync(message.Chat.Id, "Успешно!", cancellationToken: token);
    }
}