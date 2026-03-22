using CreativeCoders.Net.XmlRpc.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Represents a BidCoS interface connected to the HomeMatic CCU.
/// </summary>
/// <remarks>
/// BidCoS (Bidirectional Communication Standard) interfaces are the hardware adapters
/// through which the CCU communicates with HomeMatic RF devices. Multiple interfaces
/// can be present (e.g. for range extension). The default interface is used when a
/// device's assigned interface no longer exists.
/// Returned by <see cref="Client.IHomeMaticXmlRpcApi.ListBidcosInterfacesAsync"/>.
/// </remarks>
[PublicAPI]
public class BidcosInterface
{
    /// <summary>
    /// Gets or sets the serial number of the BidCoS interface.
    /// </summary>
    /// <value>The serial number that uniquely identifies this BidCoS interface.</value>
    [XmlRpcStructMember("ADDRESS", DefaultValue = "")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable description of the interface as configured in the interface process configuration file.
    /// </summary>
    /// <value>The textual description of the interface.</value>
    [XmlRpcStructMember("DESCRIPTION", DefaultValue = "")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value that indicates whether a communication connection to the interface is currently active.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the interface is reachable at the time of the query; otherwise, <see langword="false"/>.
    /// </value>
    [XmlRpcStructMember("CONNECTED")]
    public bool IsConnected { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether this is the default BidCoS interface.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this interface is the default; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The default interface is used as a fallback when the interface originally assigned to a device no longer exists.
    /// </remarks>
    [XmlRpcStructMember("DEFAULT")]
    public bool IsDefault { get; set; }
}