using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Devices;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Represents a single channel of a HomeMatic device.
/// </summary>
/// <param name="api">The XML-RPC API used to query parameter-set values and descriptions from the CCU.</param>
public class CcuDeviceChannel(IHomeMaticXmlRpcApi api) : CcuDeviceBase(api), ICcuDeviceChannel
{
    /// <inheritdoc />
    public required int Index { get; init; }

    /// <inheritdoc />
    public required string Group { get; init; }

    /// <inheritdoc />
    public required ChannelDirection ChannelDirection { get; init; }
}
