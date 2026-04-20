using System.Net;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Creates <see cref="CcuClient"/> instances that are wired up with the required XML-RPC and JSON-RPC clients.
/// </summary>
/// <param name="xmlRpcApiBuilder">The builder used to create <see cref="IHomeMaticXmlRpcApi"/> instances.</param>
/// <param name="jsonRpcClientBuilder">The builder used to create <see cref="IHomeMaticJsonRpcClient"/> instances.</param>
/// <param name="serviceProvider">The service provider used to resolve additional dependencies such as <see cref="ICompleteCcuDeviceBuilder"/>.</param>
public class CcuClientFactory(
    IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
    IHomeMaticJsonRpcClientBuilder jsonRpcClientBuilder,
    IServiceProvider serviceProvider) : ICcuClientFactory
{
    /// <inheritdoc />
    public ICcuClient CreateClient(string ccuName, IEnumerable<CcuDeviceKind> deviceKinds, string host, string userName,
        string password)
    {
        return new CcuClient(CreateJsonRpcClient(host, userName, password),
            CreateXmlRpcApis(deviceKinds, host, ccuName),
            serviceProvider.GetRequiredService<ICompleteCcuDeviceBuilder>());
    }

    private Dictionary<CcuDeviceKind, XmlRpcApiConnection> CreateXmlRpcApis(IEnumerable<CcuDeviceKind> deviceKinds,
        string host, string ccuName)
    {
        var xmlRpcApis = new Dictionary<CcuDeviceKind, XmlRpcApiConnection>();

        var baseUrl = new UriBuilder
        {
            Scheme = "http",
            Host = host
        };

        deviceKinds.ForEach(x =>
        {
            var apiAddress = new XmlRpcApiAddress(baseUrl.Uri, x);
            xmlRpcApis[x] = new XmlRpcApiConnection(apiAddress, CreateXmlRpcApi(apiAddress))
            {
                CcuName = ccuName
            };
        });

        return xmlRpcApis;
    }

    private IHomeMaticXmlRpcApi CreateXmlRpcApi(XmlRpcApiAddress apiAddress)
    {
        return xmlRpcApiBuilder
            .ForUrl(apiAddress.ToApiUrl())
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
