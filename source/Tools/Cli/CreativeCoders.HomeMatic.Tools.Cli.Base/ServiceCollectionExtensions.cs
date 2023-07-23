using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base;

public static class ServiceCollectionExtensions
{
    public static void AddHomeMaticCliBase(this IServiceCollection services)
    {
        services.AddSingleton<ISharedData, DefaultSharedData>();
        services.AddSingleton<ICliCommandExecutor, CliCommandExecutor>();
    }
}