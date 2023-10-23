using CreativeCoders.HomeMatic.Client.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.Client;

public static class HomeMaticClientServiceCollectionExtensions
{
    public static void AddHomeMaticClient(this IServiceCollection services)
    {
        services.TryAddSingleton<IHomeMaticClientBuilder, HomeMaticClientBuilder>();
    }
}
