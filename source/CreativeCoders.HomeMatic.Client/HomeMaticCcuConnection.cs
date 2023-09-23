using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticCcuConnection
{
    public HomeMaticCcuConnection(HomeMaticCcu ccu, IEnumerable<XmlRpcApi> xmlRpcApis,
        IHomeMaticJsonRpcApi jsonRpcApi)
    {
        Ccu = Ensure.NotNull(ccu);
        XmlRpcApis = Ensure.NotNull(xmlRpcApis);
        JsonRpcApi = Ensure.NotNull(jsonRpcApi);
    }
    
    public HomeMaticCcu Ccu { get; }

    public IEnumerable<XmlRpcApi> XmlRpcApis { get; }

    public IHomeMaticJsonRpcApi JsonRpcApi { get; }
}