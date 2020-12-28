using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core.Devices
{
    [PublicAPI]
    public interface ICcuDeviceInfo
    {
        string Address { get; }
        
        string DeviceType { get; }
        
        int Version { get; }
        
        bool IsAesActive { get; }
        
        string[] ParamSets { get; }

        bool Roaming { get; }
        
        int RfAddress { get; }
        
        string Firmware { get; }
        
        string AvailableFirmware { get; }
        
        bool CanBeUpdated { get; }
        
        DeviceFirmwareUpdateState FirmwareUpdateState { get; }
        
        string[] Children { get; }
        
        string Interface { get; }
        
        RxMode RxMode { get; }
        
        string Parent { get; }
        
        string ParentType { get; }
        
        int Index { get; }
        
        string Group { get; }
        
        ChannelDirection ChannelDirection { get; }
        
        IEnumerable<string> LinkSourceRoles { get; }
        
        IEnumerable<string> LinkTargetRoles { get; }
        
        bool IsDevice { get; }
        
        bool IsChannel { get; }
    }
}