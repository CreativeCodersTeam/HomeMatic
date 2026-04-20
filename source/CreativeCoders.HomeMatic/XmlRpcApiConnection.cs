using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class XmlRpcApiConnection(XmlRpcApiAddress address, IHomeMaticXmlRpcApi api)
{
    public XmlRpcApiAddress Address { get; } = Ensure.NotNull(address);

    public string CcuName { get; set; } = string.Empty;

    public IHomeMaticXmlRpcApi Api { get; } = Ensure.NotNull(api);
}
