using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuDevice : ICcuDeviceChannel
{
    string Name { get; }

    string DeviceType { get; }

    int Version { get; }

    int Index { get; }

    bool IsAesActive { get; }

    string Interface { get; }

    RxMode RxMode { get; }

    string Group { get; }

    int RfAddress { get; }

    string Firmware { get; }

    string AvailableFirmware { get; }

    bool CanBeUpdated { get; }

    DeviceFirmwareUpdateState FirmwareUpdateState { get; }

    bool Roaming { get; }

    ChannelDirection ChannelDirection { get; }

    string[] ParamSets { get; }

    Task<IEnumerable<ICcuDeviceChannel>> GetChannelsAsync();

    Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress);
}
