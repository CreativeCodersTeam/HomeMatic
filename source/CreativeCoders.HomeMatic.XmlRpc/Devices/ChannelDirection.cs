using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Devices;

/// <summary>
/// Specifies the direction of a channel in a direct device link.
/// </summary>
[PublicAPI]
public enum ChannelDirection
{
    /// <summary>
    /// The channel has no defined direction.
    /// </summary>
    None = 0,

    /// <summary>
    /// The channel acts as a sender in direct device links.
    /// </summary>
    Sender = 1,

    /// <summary>
    /// The channel acts as a receiver in direct device links.
    /// </summary>
    Receiver = 2
}
