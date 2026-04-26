using System.Net.Http;
using CreativeCoders.HomeMatic.Backup;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Exporting;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Provides extension methods for registering the HomeMatic services on an <see cref="IServiceCollection"/>.
/// </summary>
[PublicAPI]
public static class HomeMaticServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default HomeMatic services including the XML-RPC, JSON-RPC and exporting components.
    /// </summary>
    /// <param name="services">The service collection to register the services on.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, to allow chaining calls.</returns>
    /// <example>
    /// <code language="csharp">
    /// var services = new ServiceCollection();
    /// services.AddHomeMatic();
    /// </code>
    /// </example>
    public static IServiceCollection AddHomeMatic(this IServiceCollection services)
    {
        services.AddHomeMaticXmlRpc();
        services.AddHomeMaticJsonRpc();

        services.TryAddTransient<ICcuClientFactory, CcuClientFactory>();
        services.TryAddTransient<IMultiCcuClientFactory, MultiCcuClientFactory>();
        services.TryAddTransient<ICompleteCcuDeviceBuilder, CompleteCcuDeviceBuilder>();
        services.TryAddSingleton<IDeviceExporter, DeviceExporter>();
        services.AddHttpClient("CcuBackup")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false });
        services.TryAddTransient<ICcuBackupServiceBuilder, CcuBackupServiceBuilder>();

        return services;
    }
}
