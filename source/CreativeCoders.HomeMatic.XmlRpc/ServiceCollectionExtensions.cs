using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.XmlRpc;

public static class ServiceCollectionExtensions
{
    public static void AddHomeMaticXmlRpc(this IServiceCollection services)
    {
        services.AddXmlRpc();
        
        services.TryAddTransient<IHomeMaticXmlRpcApiBuilder, HomeMaticXmlRpcApiBuilder>();
    }
}