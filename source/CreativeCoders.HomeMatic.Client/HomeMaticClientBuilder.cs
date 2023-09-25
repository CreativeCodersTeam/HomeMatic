using System.Net;
using CreativeCoders.Core;
using CreativeCoders.Core.Enums;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticClientBuilder : IHomeMaticClientBuilder
{
    private readonly IHomeMaticXmlRpcApiBuilder _xmlRpcApiBuilder;
    
    private readonly IHomeMaticJsonRpcClientBuilder _jsonRpcClientBuilder;
    
    private List<HomeMaticCcu> _ccus = new List<HomeMaticCcu>();
    
    public HomeMaticClientBuilder(IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
        IHomeMaticJsonRpcClientBuilder jsonRpcClientBuilder)
    {
        _xmlRpcApiBuilder = Ensure.NotNull(xmlRpcApiBuilder);
        _jsonRpcClientBuilder = Ensure.NotNull(jsonRpcClientBuilder);
    }
    
    public IHomeMaticClientBuilder AddCcu(HomeMaticCcu ccu)
    {
        _ccus.Add(ccu);
        
        return this;
    }

    private HomeMaticCcuConnection CreateConnection(HomeMaticCcu ccu)
    {
        var jsonRpcApi = _jsonRpcClientBuilder
            .ForUrl(ccu.Url)
            .WithCredentials(new NetworkCredential(ccu.Username, ccu.Password))
            .Build();

        var xmlRpcApis = ccu.Systems.EnumerateFlags().Select(x =>
            {
                var uriBuilder = new UriBuilder(ccu.Url)
                {
                    Port = SystemToPort(x)
                };

                var url = uriBuilder.Uri;

                var xmlRpcApi = _xmlRpcApiBuilder
                    .ForUrl(url)
                    .Build();

                return new XmlRpcApi(x, xmlRpcApi);
            })
            .ToArray();

        return new HomeMaticCcuConnection(ccu, xmlRpcApis, jsonRpcApi);
    }

    private int SystemToPort(HomeMaticSystem system)
    {
        return system switch
        {
            HomeMaticSystem.HomeMatic => CcuRpcPorts.HomeMatic,
            HomeMaticSystem.HomeMaticIp => CcuRpcPorts.HomeMaticIp,
            HomeMaticSystem.HomeMaticIpWired => CcuRpcPorts.HomeMaticWired,
            _ => throw new ArgumentOutOfRangeException(nameof(system), system, null)
        };
    }

    public IHomeMaticClient Build()
    {
        return new HomeMaticClient(_ccus.Select(CreateConnection).ToArray());
    }
}
