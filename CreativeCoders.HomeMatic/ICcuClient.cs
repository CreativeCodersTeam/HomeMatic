using System.Net;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public interface ICcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    Task<ICcuDevice> GetDeviceAsync(string address);
}

public class CcuClient(
    IHomeMaticJsonRpcClient jsonRpcClient,
    IDictionary<HomeMaticDeviceSystems, IHomeMaticXmlRpcApi> xmlRpcApis) : ICcuClient
{
    public Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICcuDevice> GetDeviceAsync(string address)
    {
        throw new NotImplementedException();
    }
}

public interface ICcuClientFactory
{
    ICcuClient CreateClient(HomeMaticDeviceSystems deviceSystems, string host, string userName, string password);
}

public class CcuClientFactory(
    IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
    IHomeMaticJsonRpcClientBuilder jsonRpcClientBuilder) : ICcuClientFactory
{
    public ICcuClient CreateClient(HomeMaticDeviceSystems deviceSystems, string host, string userName, string password)
    {
        return new CcuClient(CreateJsonRpcClient(host, userName, password),
            CreateXmlRpcApis(deviceSystems, host));
    }

    private IDictionary<HomeMaticDeviceSystems, IHomeMaticXmlRpcApi> CreateXmlRpcApis(
        HomeMaticDeviceSystems deviceSystems, string host)
    {
        var xmlRpcApis = new Dictionary<HomeMaticDeviceSystems, IHomeMaticXmlRpcApi>();

        var baseUrl = new UriBuilder
        {
            Scheme = "http",
            Host = host
        };

        if (deviceSystems.HasFlag(HomeMaticDeviceSystems.HomeMatic))
        {
            var xmlRpcApiAddress = new XmlRpcApiAddress(baseUrl.Uri, HomeMaticDeviceSystems.HomeMatic);
            xmlRpcApis[HomeMaticDeviceSystems.HomeMatic] = CreateXmlRpcApi(xmlRpcApiAddress);
        }

        return xmlRpcApis;
    }

    private IHomeMaticXmlRpcApi CreateXmlRpcApi(XmlRpcApiAddress xmlRpcApiAddress)
    {
        return xmlRpcApiBuilder
            .ForUrl(xmlRpcApiAddress.ToApiUrl())
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
