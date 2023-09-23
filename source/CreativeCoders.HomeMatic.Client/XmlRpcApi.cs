using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class XmlRpcApi
{
    public XmlRpcApi(HomeMaticSystem system, IHomeMaticXmlRpcApi api)
    {
        System = Ensure.NotNull(system);
        Api = Ensure.NotNull(api);
    }

    public HomeMaticSystem System { get; }

    public IHomeMaticXmlRpcApi Api { get; }
}
