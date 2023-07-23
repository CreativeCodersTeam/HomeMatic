using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base;

public static class ServiceCollectionExtensions
{
    public static void AddHomeMaticCliBase(this IServiceCollection services)
    {
        services.TryAddSingleton<ISharedData, DefaultSharedData>();
        services.TryAddSingleton<ICliCommandExecutor, CliCommandExecutor>();
    }
}