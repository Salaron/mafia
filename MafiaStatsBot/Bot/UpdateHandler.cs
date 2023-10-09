using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MafiaStatsBot.Bot;

public class UpdateHandler(ILogger<UpdateHandler> logger, IEnumerable<ICommandHandler> commandHandlers)
    : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: жесть какая-то
            if (update.Message?.Entities?.First().Type == MessageEntityType.BotCommand ||
                update.Message?.CaptionEntities?.First().Type == MessageEntityType.BotCommand)
            {
                var command = (update.Message?.EntityValues ?? update.Message?.CaptionEntityValues).First()
                    .Replace(BotService.BotName, "");

                var commandHandler =
                    commandHandlers.OfType<BaseCommandHandler>().FirstOrDefault(c => command.Contains(c.Command));
                if (commandHandler != null)
                    await commandHandler.ExecuteAsync(botClient, update.Message, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "process command error (0E9CD383-2021-4F0E-9A96-C8BEF6AB6D83)");
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogWarning("HandleError: {ErrorMessage}", errorMessage);

        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}