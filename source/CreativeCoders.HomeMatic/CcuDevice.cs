using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Represents a HomeMatic device with its channels, combining device-level metadata with parameter-set access.
/// </summary>
/// <param name="api">The XML-RPC API used to query parameter-set values and descriptions from the CCU.</param>
public class CcuDevice(IHomeMaticXmlRpcApi api) : CcuDeviceBase(api), ICcuDevice
{
    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public required RxModes RxMode { get; init; }

    /// <inheritdoc />
    public required int RfAddress { get; init; }

    /// <inheritdoc />
    public required string Firmware { get; init; }

    /// <inheritdoc />
    public required string AvailableFirmware { get; init; }

    /// <inheritdoc />
    public required bool CanBeUpdated { get; init; }

    /// <inheritdoc />
    public required DeviceFirmwareUpdateState FirmwareUpdateState { get; init; }

    /// <inheritdoc />
    public required IEnumerable<ICcuDeviceChannel> Channels { get; init; }

    /// <summary>
    /// Asynchronously retrieves a single channel by its address.
    /// </summary>
    /// <param name="channelAddress">The full address of the channel (for example <c>"ABC0001234:1"</c>).</param>
    /// <returns>A task that yields the matching <see cref="ICcuDeviceChannel"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no channel with the specified address exists on this device.</exception>
    public Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress)
    {
        var channel = Channels.FirstOrDefault(x => x.Uri.Address == channelAddress);
        return channel != null
            ? Task.FromResult(channel)
            : throw new KeyNotFoundException(
                $"Channel with address '{channelAddress}' not found.");
    }
}
