using CreativeCoders.HomeMatic.JsonRpc.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.JsonRpc;

public static class HomeMaticJsonRpcServiceCollectionExtensions
{
    public static void AddHomeMaticJsonRpc(this IServiceCollection services)
    {
        services.AddJsonRpcClient();
        
        services.TryAddTransient<IHomeMaticJsonRpcApiBuilder, HomeMaticJsonRpcApiBuilder>();
        
        services.TryAddTransient<IHomeMaticJsonRpcClientBuilder, HomeMaticJsonRpcClientBuilder>();
    }
}
