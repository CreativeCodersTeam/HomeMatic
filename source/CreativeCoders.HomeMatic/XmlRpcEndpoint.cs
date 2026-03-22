using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic;

public class XmlRpcEndpoint
{
    public XmlRpcEndpoint(Uri baseUrl, CcuDeviceKind deviceKind)
    {
        BaseUrl = Ensure.NotNull(baseUrl);
        DeviceKind = deviceKind;
    }

    public Uri ToApiUrl()
    {
        var uriBuilder = new UriBuilder(BaseUrl)
        {
            Port = DeviceKind.ToPort()
        };

        return uriBuilder.Uri;
    }

    public Uri BaseUrl { get; }

    public CcuDeviceKind DeviceKind { get; }
}
