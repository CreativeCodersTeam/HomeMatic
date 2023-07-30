using CreativeCoders.HomeMatic.JsonRpc.Client;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.JsonRpc;

public static class JsonRpcServiceCollectionExtensions
{
    public static IServiceCollection AddJsonRpcClient(this IServiceCollection services, string url)
    {
        services.AddHttpClient();

        return services;
    }
}
