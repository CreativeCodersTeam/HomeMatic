using System.Diagnostics.CodeAnalysis;
using CreativeCoders.Core.IO;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Extension methods for registering the firmware backup feature on an <see cref="IServiceCollection"/>.
/// </summary>
[PublicAPI]
public static class FirmwareBackupServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IFirmwareBackupClientFactory"/> and the named <see cref="HttpClient"/>
    /// used to talk to a HomeMatic CCU.
    /// </summary>
    /// <param name="services">The service collection to register the services on.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow chaining calls.</returns>
    [SuppressMessage("csharpsquid", "S4830:Server certificate validation should not be disabled",
        Justification =
            "Only used for explicitly opting in to accepting any server certificate, e.g. for self-signed certificates on a CCU.")]
    public static IServiceCollection AddHomeMaticFirmwareBackup(this IServiceCollection services)
    {
        services.AddFileSystem();

        services.AddHttpClient(FirmwareBackupClientFactory.HttpClientName);

        services
            .AddHttpClient(FirmwareBackupClientFactory.HttpClientNameAcceptAnyCertificate)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            });

        services.TryAddTransient<IFirmwareBackupClientFactory, FirmwareBackupClientFactory>();

        return services;
    }
}
