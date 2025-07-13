using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class XmlRpcApiConnection(XmlRpcEndpoint endpoint, IHomeMaticXmlRpcApi api)
{
    public XmlRpcEndpoint Endpoint { get; } = Ensure.NotNull(endpoint);

    public string CcuName { get; set; } = string.Empty;

    public IHomeMaticXmlRpcApi Api { get; } = Ensure.NotNull(api);
}
