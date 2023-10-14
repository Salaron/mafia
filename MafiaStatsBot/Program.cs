using MafiaLib;
using MafiaLib.Database;
using MafiaLib.GameHistoryImporter;
using MafiaLib.Statistic;
using MafiaStatsBot;
using MafiaStatsBot.Bot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

Environment.SetEnvironmentVariable("LogsDir", AppContext.BaseDirectory);

builder.Services.AddSerilog((_, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddOptions<SharedConfig>()
    .Bind(builder.Configuration)
    .ValidateOnStart();

builder.Services.AddOptions<MafiaStatsConfig>()
    .Bind(builder.Configuration.GetSection("StatsBot"))
    .Bind(builder.Configuration)
    .ValidateOnStart();

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var config = sp.GetRequiredService<IOptions<MafiaStatsConfig>>();
        var options = new TelegramBotClientOptions(config.Value.Token);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
});

builder.Services.AddCommandHandlers();
builder.Services.AddScoped<StatsProvider>();

builder.Services.AddScoped<HistoryImporter>();
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddHostedService<BotService>();

var host = builder.Build();
await host.RunAsync();