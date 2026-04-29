using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base;

public static class CliBaseServiceCollectionExtensions
{
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

        services.AddHomeMatic();
    }
}
