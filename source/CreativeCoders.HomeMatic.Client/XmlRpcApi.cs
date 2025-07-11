using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class XmlRpcApi
{
    public XmlRpcApi(HomeMaticDeviceSystems deviceSystems, IHomeMaticXmlRpcApi api)
    {
        DeviceSystems = Ensure.NotNull(deviceSystems);
        Api = Ensure.NotNull(api);
    }

    public HomeMaticDeviceSystems DeviceSystems { get; }

    public IHomeMaticXmlRpcApi Api { get; }
}
