using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class XmlRpcApi
{
    public XmlRpcApi(HomeMaticDeviceSystem deviceSystem, IHomeMaticXmlRpcApi api)
    {
        DeviceSystem = Ensure.NotNull(deviceSystem);
        Api = Ensure.NotNull(api);
    }

    public HomeMaticDeviceSystem DeviceSystem { get; }

    public IHomeMaticXmlRpcApi Api { get; }
}
