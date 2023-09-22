using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticCcuConnection
{
    public HomeMaticCcuConnection(HomeMaticCcu ccu, IHomeMaticXmlRpcApi xmlRpcApi,
        IHomeMaticJsonRpcApi jsonRpcApi)
    {
        Ccu = Ensure.NotNull(ccu);
        XmlRpcApi = Ensure.NotNull(xmlRpcApi);
        JsonRpcApi = Ensure.NotNull(jsonRpcApi);
    }
    
    public HomeMaticCcu Ccu { get; }

    public IHomeMaticXmlRpcApi XmlRpcApi { get; }

    public IHomeMaticJsonRpcApi JsonRpcApi { get; }
}
