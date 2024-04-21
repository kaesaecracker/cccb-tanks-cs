using System.Text;
using DisplayCommands.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DisplayCommands;

public static class DisplayExtensions
{
    static DisplayExtensions()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static IServiceCollection AddDisplay(
        this IServiceCollection services,
        IConfigurationSection? configurationSection = null
    )
    {
        services.AddSingleton<IDisplayConnection, DisplayConnection>();
        if (configurationSection != null)
            services.Configure<DisplayConfiguration>(configurationSection);
        return services;
    }
}