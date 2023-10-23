using System;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.XmlRpc;

public class XmlRpcApiAddress
{
    public XmlRpcApiAddress(Uri baseUrl, HomeMaticDeviceSystem deviceSystem)
    {
        BaseUrl = Ensure.NotNull(baseUrl);
        DeviceSystem = deviceSystem;
    }

    public Uri ToApiUrl()
    {
        var uriBuilder = new UriBuilder(BaseUrl);
        uriBuilder.Port = DeviceSystem.ToPort();
        
        return uriBuilder.Uri;
    }
    
    public Uri BaseUrl { get; }

    public HomeMaticDeviceSystem DeviceSystem { get; }
}
