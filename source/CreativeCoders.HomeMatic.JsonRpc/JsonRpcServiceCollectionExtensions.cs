using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;
using CreativeCoders.HomeMatic.JsonRpc.RpcClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.JsonRpc;

public static class JsonRpcServiceCollectionExtensions
{
    public static IServiceCollection AddJsonRpcClient(this IServiceCollection services)
    {
        services.AddHttpClient();
        
        services.TryAddTransient<IJsonRpcClient, JsonRpcClient>();
        
        services.TryAddSingleton<IJsonRpcClientFactory, JsonRpcClientFactory>();
        
        services.AddProxyBuilder();
        
        services.TryAddSingleton(typeof(IJsonRpcApiBuilder<>), typeof(JsonRpcApiBuilder<>));

        return services;
    }
    
    public static IServiceCollection AddHomeMaticJsonRpcClient(this IServiceCollection services)
    {
        services.AddJsonRpcClient();
        
        services.TryAddTransient<IHomeMaticJsonRpcApi, HomeMaticJsonRpcApi>();

        return services;
    }
}
