using CreativeCoders.HomeMatic.JsonRpc.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.JsonRpc;

public static class JsonRpcServiceCollectionExtensions
{
    public static IServiceCollection AddHomeMaticJsonRpcClient(this IServiceCollection services)
    {
        services.AddHttpClient();
        
        services.TryAddTransient<IJsonRpcClient, JsonRpcClient>();
        
        services.TryAddTransient<IHomeMaticJsonRpcApi, HomeMaticJsonRpcApi>();

        return services;
    }
}
