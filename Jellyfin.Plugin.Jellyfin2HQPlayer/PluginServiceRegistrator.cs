using Jellyfin.Plugin.Jellyfin2HQPlayer.Services;
using Jellyfin.Plugin.Jellyfin2HQPlayer.EntryPoint;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Jellyfin2HQPlayer;

public sealed class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<PathIndexService>();
        serviceCollection.AddHostedService<StartupBuildService>();
        serviceCollection.AddHostedService<LibraryItemEventLoggerService>();
    }
}
