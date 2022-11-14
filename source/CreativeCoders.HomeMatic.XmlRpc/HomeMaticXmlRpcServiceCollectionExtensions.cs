using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using CreativeCoders.Net.XmlRpc;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.XmlRpc;

public static class HomeMaticXmlRpcServiceCollectionExtensions
{
    public static void AddHomeMaticXmlRpc(this IServiceCollection services)
    {
        services.AddXmlRpc();

        services.AddSingleton<ICcuXmlRpcEventServer, CcuXmlRpcEventServer>();
        
        services.AddSingleton<ICcuXmlRpcEventServerFactory, CcuXmlRpcEventServerFactory>();

        services.AddSingleton<IHomeMaticXmlRpcApiBuilder, HomeMaticXmlRpcApiBuilder>();
    }
}
