using MafiaLib;
using MafiaLib.Database;
using MafiaLib.GameHistoryImporter;
using MafiaUserBot;
using MafiaUserBot.UserBot;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddSerilog((_, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddOptions<SharedConfig>()
    .Bind(builder.Configuration)
    .ValidateOnStart();

builder.Services.AddOptions<UserBotConfig>()
    .Bind(builder.Configuration.GetSection("UserBot"))
    .Bind(builder.Configuration)
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
});

builder.Services.AddUpdateHandlers();
builder.Services.AddScoped<HistoryImporter>();
builder.Services.AddHostedService<UserBotService>();

var host = builder.Build();
await host.RunAsync();
