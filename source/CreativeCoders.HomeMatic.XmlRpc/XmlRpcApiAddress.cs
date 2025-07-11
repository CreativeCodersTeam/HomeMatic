using System;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.XmlRpc;

public class XmlRpcApiAddress
{
    public XmlRpcApiAddress(Uri baseUrl, HomeMaticDeviceSystems deviceSystems)
    {
        BaseUrl = Ensure.NotNull(baseUrl);
        DeviceSystems = deviceSystems;
    }

    public Uri ToApiUrl()
    {
        var uriBuilder = new UriBuilder(BaseUrl);
        uriBuilder.Port = DeviceSystems.ToPort();
        
        return uriBuilder.Uri;
    }
    
    public Uri BaseUrl { get; }

    public HomeMaticDeviceSystems DeviceSystems { get; }
}
