using System;
using CreativeCoders.Core;
using CreativeCoders.Net.XmlRpc.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

public class CcuXmlRpcEventServerFactory : ICcuXmlRpcEventServerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CcuXmlRpcEventServerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = Ensure.NotNull(serviceProvider, nameof(serviceProvider));
    }
    
    public ICcuXmlRpcEventServer CreateServer()
    {
        return new CcuXmlRpcEventServer(
            _serviceProvider.GetRequiredService<IXmlRpcServer>(),
            _serviceProvider.GetRequiredService<ILogger<CcuXmlRpcEventServer>>());
    }

    public ICcuXmlRpcEventServer CreateServer(IXmlRpcServer xmlRpcServer)
    {
        return new CcuXmlRpcEventServer(
            xmlRpcServer,
            _serviceProvider.GetRequiredService<ILogger<CcuXmlRpcEventServer>>());
    }
}
