using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class CcuDevice(IHomeMaticXmlRpcApi api) : CcuDeviceChannel, ICcuDevice
{
    public string Name { get; set; } = string.Empty;

    public required string DeviceType { get; init; }

    public required int Version { get; init; }

    public required int Index { get; init; }

    public required bool IsAesActive { get; init; }

    public required string Interface { get; init; }

    public required RxMode RxMode { get; init; }

    public required string Group { get; init; }

    public required int RfAddress { get; init; }

    public required string Firmware { get; init; }

    public required string AvailableFirmware { get; init; }

    public required bool CanBeUpdated { get; init; }

    public required DeviceFirmwareUpdateState FirmwareUpdateState { get; init; }

    public required bool Roaming { get; init; }

    public required ChannelDirection ChannelDirection { get; init; }

    public required string[] ParamSets { get; init; }

    public Task<IEnumerable<ICcuDeviceChannel>> GetChannelsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress)
    {
        throw new NotImplementedException();
    }
}
