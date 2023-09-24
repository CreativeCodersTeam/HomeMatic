using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.JsonRpc;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticCcuConnection
{
    public HomeMaticCcuConnection(HomeMaticCcu ccu, IEnumerable<XmlRpcApi> xmlRpcApis,
        IHomeMaticJsonRpcClient jsonRpcApi)
    {
        Ccu = Ensure.NotNull(ccu);
        XmlRpcApis = Ensure.NotNull(xmlRpcApis);
        JsonRpcClient = Ensure.NotNull(jsonRpcApi);
    }
    
    public HomeMaticCcu Ccu { get; }

    public IEnumerable<XmlRpcApi> XmlRpcApis { get; }

    public IHomeMaticJsonRpcClient JsonRpcClient { get; }
}