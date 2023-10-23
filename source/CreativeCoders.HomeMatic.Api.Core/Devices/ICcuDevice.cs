using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core.Devices;

[PublicAPI]
public interface ICcuDevice : ICcuDeviceBase
{
    int RfAddress { get; }
        
    string Firmware { get; }
        
    string AvailableFirmware { get; }
        
    bool CanBeUpdated { get; }
        
    DeviceFirmwareUpdateState FirmwareUpdateState { get; }
        
    IReadOnlyList<ICcuDeviceChannel> Channels { get; }
        
    string Interface { get; }

    RxMode RxMode { get; }
}