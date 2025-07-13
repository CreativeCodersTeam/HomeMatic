using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuDevice : ICcuDeviceBase
{
    string Name { get; }

    RxMode RxMode { get; }

    int RfAddress { get; }

    string Firmware { get; }

    string AvailableFirmware { get; }

    bool CanBeUpdated { get; }

    DeviceFirmwareUpdateState FirmwareUpdateState { get; }

    Task<IEnumerable<ICcuDeviceChannel>> GetChannelsAsync();

    Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress);
}
