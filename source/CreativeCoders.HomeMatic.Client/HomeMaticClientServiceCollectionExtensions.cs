using CreativeCoders.HomeMatic.Client.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Client;

public static class HomeMaticClientServiceCollectionExtensions
{
    public static void AddHomeMaticClient(this IServiceCollection services)
    {
        services.AddSingleton<IHomeMaticClientBuilder, HomeMaticClientBuilder>();
    }
}
