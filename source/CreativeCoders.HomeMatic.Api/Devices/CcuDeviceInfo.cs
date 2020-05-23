using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Devices
{
    [UsedImplicitly]
    public class CcuDeviceInfo : ICcuDeviceInfo
    {
        public string Address { get; set; }
        
        public string DeviceType { get; set; }
        
        public string[] Children { get; set; }
        
        public string Parent { get; set; }
        
        public string ParentType { get; set; }
        
        public int Version { get; set; }
        
        public int Index { get; set; }
        
        public bool IsAesActive { get; set; }
        
        public string Interface { get; set; }
        
        public string[] ParamSets { get; set; }
        
        public RxMode RxMode { get; set; }
        
        public string Group { get; set; }
        
        public int RfAddress { get; set; }
        
        public string Firmware { get; set; }
        
        public string AvailableFirmware { get; set; }
        
        public bool CanBeUpdated { get; set; }
        
        public DeviceFirmwareUpdateState FirmwareUpdateState { get; set; }
        
        public bool Roaming { get; set; }
        
        public ChannelDirection ChannelDirection { get; set; }
        
        public IEnumerable<string> LinkSourceRoles { get; set; }
        
        public IEnumerable<string> LinkTargetRoles { get; set; }
        
        public bool IsDevice { get; set; }
        
        public bool IsChannel { get; set; }
    }
}