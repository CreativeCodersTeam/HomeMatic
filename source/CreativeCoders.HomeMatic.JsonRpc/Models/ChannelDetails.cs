using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.JsonRpc.Models;

/// <summary>
/// Represents a channel of a device as returned by the CCU JSON-RPC method <c>Device.listAllDetail</c>.
/// </summary>
/// <remarks>
/// Further JSON fields of a channel are ignored during deserialization.
/// </remarks>
[UsedImplicitly]
[PublicAPI]
public class ChannelDetails
{
    /// <summary>
    /// Gets or sets the CCU-internal identifier of the channel.
    /// </summary>
    /// <value>The channel identifier, or <see langword="null"/> if the CCU did not provide one.</value>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the channel as configured on the CCU.
    /// </summary>
    /// <value>The channel name (e.g. <c>Living room light:1</c>), or <see langword="null"/> if the CCU did not provide one.</value>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the address of the channel.
    /// </summary>
    /// <value>The channel address (e.g. <c>000A1B2C3D4E5F:1</c>), or <see langword="null"/> if the CCU did not provide one.</value>
    public string? Address { get; set; }
}
