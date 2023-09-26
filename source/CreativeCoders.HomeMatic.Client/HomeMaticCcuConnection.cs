using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.JsonRpc;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticCcuConnection
{
    public HomeMaticCcuConnection(HomeMaticCcuConnectionInfo ccuConnectionInfo, IEnumerable<XmlRpcApi> xmlRpcApis,
        IHomeMaticJsonRpcClient jsonRpcApi)
    {
        Info = Ensure.NotNull(ccuConnectionInfo);
        XmlRpcApis = Ensure.NotNull(xmlRpcApis);
        JsonRpcClient = Ensure.NotNull(jsonRpcApi);
    }
    
    public HomeMaticCcuConnectionInfo Info { get; }

    public IEnumerable<XmlRpcApi> XmlRpcApis { get; }

    public IHomeMaticJsonRpcClient JsonRpcClient { get; }
}