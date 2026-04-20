using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Exporting;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic;

public static class HomeMaticServiceCollectionExtensions
{
    public static IServiceCollection AddHomeMatic(this IServiceCollection services)
    {
        services.AddHomeMaticXmlRpc();
        services.AddHomeMaticJsonRpc();

        services.TryAddTransient<ICcuClientFactory, CcuClientFactory>();
        services.TryAddTransient<IMultiCcuClientFactory, MultiCcuClientFactory>();
        services.TryAddTransient<ICompleteCcuDeviceBuilder, CompleteCcuDeviceBuilder>();
        services.TryAddSingleton<IDeviceExporter, DeviceExporter>();

        return services;
    }
}
