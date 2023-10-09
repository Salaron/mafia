using MafiaStatsBot.Bot;
using MafiaStatsBot.Commands;

namespace MafiaStatsBot;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        services.AddTransient<ICommandHandler, LastRolesCommandHandler>();
        services.AddTransient<ICommandHandler, ImportCommandHandler>();
        services.AddTransient<ICommandHandler, StatsCommandHandler>();
        services.AddTransient<ICommandHandler, TotalStatsCommandHandler>();

        return services;
    }
}