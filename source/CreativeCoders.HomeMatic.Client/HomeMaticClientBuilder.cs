using System.Net;
using CreativeCoders.Core;
using CreativeCoders.Core.Enums;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticClientBuilder : IHomeMaticClientBuilder
{
    private readonly IHomeMaticXmlRpcApiBuilder _xmlRpcApiBuilder;
    
    private readonly IHomeMaticJsonRpcClientBuilder _jsonRpcClientBuilder;
    
    private List<HomeMaticCcuConnectionInfo> _ccus = new List<HomeMaticCcuConnectionInfo>();
    
    public HomeMaticClientBuilder(IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
        IHomeMaticJsonRpcClientBuilder jsonRpcClientBuilder)
    {
        _xmlRpcApiBuilder = Ensure.NotNull(xmlRpcApiBuilder);
        _jsonRpcClientBuilder = Ensure.NotNull(jsonRpcClientBuilder);
    }
    
    public IHomeMaticClientBuilder AddCcu(HomeMaticCcuConnectionInfo ccuConnectionInfo)
    {
        _ccus.Add(ccuConnectionInfo);
        
        return this;
    }

    private HomeMaticCcuConnection CreateConnection(HomeMaticCcuConnectionInfo ccuConnectionInfo)
    {
        var jsonRpcApi = _jsonRpcClientBuilder
            .ForUrl(ccuConnectionInfo.Url)
            .WithCredentials(new NetworkCredential(ccuConnectionInfo.Username, ccuConnectionInfo.Password))
            .Build();

        var xmlRpcApis = ccuConnectionInfo.Systems.EnumerateFlags().Select(x =>
            {
                var xmlRpcApi = _xmlRpcApiBuilder
                    .ForUrl(new XmlRpcApiAddress(ccuConnectionInfo.Url, x))
                    .Build();

                return new XmlRpcApi(x, xmlRpcApi);
            })
            .ToArray();

        return new HomeMaticCcuConnection(ccuConnectionInfo, xmlRpcApis, jsonRpcApi);
    }

    public IHomeMaticClient Build()
    {
        return new HomeMaticClient(_ccus.Select(CreateConnection).ToArray());
    }
}
