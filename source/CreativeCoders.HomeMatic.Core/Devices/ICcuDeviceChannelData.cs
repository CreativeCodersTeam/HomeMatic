using CreativeCoders.HomeMatic.XmlRpc.Devices;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Defines the channel-specific data of a HomeMatic device channel.
/// </summary>
[PublicAPI]
public interface ICcuDeviceChannelData : ICcuDeviceBaseData
{
    /// <summary>
    /// Gets the zero-based index of the channel within its parent device.
    /// </summary>
    /// <value>The channel index.</value>
    int Index { get; }

    /// <summary>
    /// Gets the paired channel address when the channel belongs to a button group.
    /// </summary>
    /// <value>The address of the paired channel, or an empty string if none.</value>
    string Group { get; }

    /// <summary>
    /// Gets the direction of the channel in a direct device link.
    /// </summary>
    /// <value>One of the enumeration values that specifies the channel direction.</value>
    ChannelDirection ChannelDirection { get; }
}
