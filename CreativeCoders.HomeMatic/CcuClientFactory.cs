using System.Net;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class CcuClientFactory(
    IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
    IHomeMaticJsonRpcClientBuilder jsonRpcClientBuilder) : ICcuClientFactory
{
    public ICcuClient CreateClient(IEnumerable<CcuDeviceKind> deviceKinds, string host, string userName,
        string password)
    {
        return new CcuClient(CreateJsonRpcClient(host, userName, password),
            CreateXmlRpcApis(deviceKinds, host));
    }

    private Dictionary<CcuDeviceKind, IHomeMaticXmlRpcApi> CreateXmlRpcApis(
        IEnumerable<CcuDeviceKind> deviceKinds, string host)
    {
        var xmlRpcApis = new Dictionary<CcuDeviceKind, IHomeMaticXmlRpcApi>();

        var baseUrl = new UriBuilder
        {
            Scheme = "http",
            Host = host
        };

        deviceKinds.ForEach(x =>
        {
            var xmlRpcEndpoint = new XmlRpcEndpoint(baseUrl.Uri, x);
            xmlRpcApis[x] = CreateXmlRpcApi(xmlRpcEndpoint);
        });

        return xmlRpcApis;
    }

    private IHomeMaticXmlRpcApi CreateXmlRpcApi(XmlRpcEndpoint xmlRpcEndpoint)
    {
        return xmlRpcApiBuilder
            .ForUrl(xmlRpcEndpoint.ToApiUrl())
            .Build();
    }

    private IHomeMaticJsonRpcClient CreateJsonRpcClient(string host, string userName, string password)
    {
        var jsonRpcUriBuilder = new UriBuilder
        {
            Scheme = "http",
            Host = host,
            Port = 80
        };

        return jsonRpcClientBuilder
            .WithCredentials(new NetworkCredential(userName, password))
            .ForUrl(jsonRpcUriBuilder.Uri)
            .Build();
    }
}
