using CreativeCoders.HomeMatic.Client;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
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
        services.TryAddSingleton<ICliCommandExecutor, CliCommandExecutor>();
        
        services.TryAddSingleton<ICcuConnectionsStore, CcuConnectionsStore>();
        
        services.TryAddSingleton<ICliHomeMaticClientBuilder, CliHomeMaticClientBuilder>();

        services.AddHomeMaticClient();
    }
}