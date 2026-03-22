using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Provides extension methods for registering HomeMatic XML-RPC services with the dependency injection container.
/// </summary>
public static class XmlRpcServiceCollectionExtensions
{
    /// <summary>
    /// Registers the HomeMatic XML-RPC client services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <remarks>
    /// Registers the underlying XML-RPC infrastructure and the <see cref="Client.IHomeMaticXmlRpcApiBuilder"/>
    /// so that callers can construct typed <see cref="Client.IHomeMaticXmlRpcApi"/> instances
    /// for individual CCU interface processes.
    /// </remarks>
    public static void AddHomeMaticXmlRpc(this IServiceCollection services)
    {
        services.AddXmlRpc();
        
        services.TryAddTransient<IHomeMaticXmlRpcApiBuilder, HomeMaticXmlRpcApiBuilder>();
    }
}