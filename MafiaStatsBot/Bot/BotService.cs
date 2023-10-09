using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MafiaStatsBot.Bot;

public class BotService(ITelegramBotClient botClient,
        ILogger<BotService> logger,
        IServiceProvider serviceProvider)
    : BackgroundService
{
    public static string BotName { get; set; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var errorCount = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>(),
                };

                var me = await botClient.GetMeAsync(cancellationToken);
                BotName = me.Username;
                logger.LogInformation($"Start receiving updates for {me.Username}");

                var commandHandlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ICommandHandler>>();
                await RegisterBotCommandsAsync(commandHandlers);
                // Start receiving updates
                await botClient.ReceiveAsync(
                    updateHandler: scope.ServiceProvider.GetRequiredService<IUpdateHandler>(),
                    receiverOptions: receiverOptions,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "update error (021D0C56-A2D1-4333-AC4E-565F947062BA)");
                errorCount++;

                await Task.Delay(2_000, cancellationToken);
                if (errorCount > 10)
                {
                    logger.LogError("Stopping");
                    throw;
                }
            }
        }
    }

    private async Task RegisterBotCommandsAsync(IEnumerable<ICommandHandler> commandHandlers)
    {
        var commandHandlersBase = commandHandlers.OfType<BaseCommandHandler>().ToList();
        var scopes = commandHandlersBase
            .Where(c => c.HelpText != string.Empty)
            .SelectMany(s => s.CommandScopes)
            .DistinctBy(s => s.Type);

        foreach (var scope in scopes)
        {
            var botCommands = commandHandlersBase
                .Where(c => c.CommandScopes.Select(s => s.Type).Contains(scope.Type))
                .Select(c => new BotCommand
                {
                    Command = c.Command,
                    Description = c.HelpText
                });

            await botClient.SetMyCommandsAsync(botCommands, scope);
        }
    }
}