using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Bundles an <see cref="XmlRpcApiAddress"/> and the associated <see cref="IHomeMaticXmlRpcApi"/>
/// together with the logical CCU name.
/// </summary>
/// <param name="address">The address identifying the XML-RPC endpoint on the CCU.</param>
/// <param name="api">The XML-RPC API instance used to talk to that endpoint.</param>
public class XmlRpcApiConnection(XmlRpcApiAddress address, IHomeMaticXmlRpcApi api)
{
    /// <summary>
    /// Gets the address of the XML-RPC endpoint on the CCU.
    /// </summary>
    /// <value>The <see cref="XmlRpcApiAddress"/> of the endpoint.</value>
    public XmlRpcApiAddress Address { get; } = Ensure.NotNull(address);

    /// <summary>
    /// Gets or sets the logical name of the CCU this connection belongs to.
    /// </summary>
    /// <value>The CCU name, or an empty string if not set.</value>
    public string CcuName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the XML-RPC API instance used to communicate with the CCU endpoint.
    /// </summary>
    /// <value>The <see cref="IHomeMaticXmlRpcApi"/> instance.</value>
    public IHomeMaticXmlRpcApi Api { get; } = Ensure.NotNull(api);
}
