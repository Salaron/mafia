using MafiaUserBot.UpdateHandlers;
using MafiaUserBot.UserBot;

namespace MafiaUserBot;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUpdateHandlers(this IServiceCollection services)
    {
        services.AddTransient<IUpdateHandler, AdRemover>();
        services.AddTransient<IUpdateHandler, MarkAsRead>();
        services.AddTransient<IUpdateHandler, GameResultSaver>();

        return services;
    }
}