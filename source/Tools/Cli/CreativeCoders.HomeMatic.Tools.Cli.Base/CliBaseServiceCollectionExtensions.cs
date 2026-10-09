using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Events;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base;

/// <summary>
/// Provides extension methods for registering the shared services of the HomeMatic CLI tools.
/// </summary>
public static class CliBaseServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared services of the HomeMatic CLI tools and the HomeMatic client services.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <remarks>
    /// Registers the CCU connection store, the HomeMatic client builders, the data serializers and output writer,
    /// the <see cref="ICallbackEndpointResolver"/> and the <see cref="IConsoleCancelKeySource"/> as singletons,
    /// unless a registration for the service type already exists.
    /// </remarks>
    public static void AddHomeMaticCliBase(this IServiceCollection services)
    {
        services.TryAddSingleton<ISharedData, DefaultSharedData>();
        services.TryAddSingleton<ICcuConnectionsStore, CcuConnectionsStore>();

        services.TryAddSingleton<ICliHomeMaticClientBuilder, CliHomeMaticClientBuilder>();

        services.TryAddSingleton<IMultiCcuClient>(sp =>
            sp.GetRequiredService<ICliHomeMaticClientBuilder>().BuildMultiCcuClient());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDataSerializer, JsonDataSerializer>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDataSerializer, YamlDataSerializer>());
        services.TryAddSingleton<IDataSerializerFactory, DataSerializerFactory>();
        services.TryAddSingleton<IDataOutputWriter, DataOutputWriter>();

        services.TryAddSingleton<ICallbackEndpointResolver, CallbackEndpointResolver>();
        services.TryAddSingleton<IConsoleCancelKeySource, ConsoleCancelKeySource>();

        services.AddHomeMatic();
    }
}
