using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class CcuDevice(IHomeMaticXmlRpcApi api) : CcuDeviceBase(api), ICcuDevice
{
    public string Name { get; set; } = string.Empty;

    public required RxMode RxMode { get; init; }

    public required int RfAddress { get; init; }

    public required string Firmware { get; init; }

    public required string AvailableFirmware { get; init; }

    public required bool CanBeUpdated { get; init; }

    public required DeviceFirmwareUpdateState FirmwareUpdateState { get; init; }

    public required IEnumerable<ICcuDeviceChannel> Channels { get; init; }

    public Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress)
    {
        var channel = Channels.FirstOrDefault(x => x.Uri.Address == channelAddress);
        return channel != null
            ? Task.FromResult(channel)
            : throw new KeyNotFoundException(
                $"Channel with address '{channelAddress}' not found.");
    }
}
