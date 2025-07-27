using System.Collections.Generic;
using CreativeCoders.HomeMatic.Api.Core.Devices;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Devices;

[PublicAPI]
public class CcuDeviceInfo : ICcuDeviceInfo
{
    public string Address { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;

    public string[] Children { get; set; } = [];

    public string Parent { get; set; } = string.Empty;

    public string ParentType { get; set; } = string.Empty;

    public int Version { get; set; }

    public int Index { get; set; }

    public bool IsAesActive { get; set; }

    public string Interface { get; set; } = string.Empty;

    public string[] ParamSets { get; set; } = [];

    public RxMode RxMode { get; set; }

    public string Group { get; set; } = string.Empty;

    public int RfAddress { get; set; }

    public string Firmware { get; set; } = string.Empty;

    public string AvailableFirmware { get; set; } = string.Empty;

    public bool CanBeUpdated { get; set; }

    public DeviceFirmwareUpdateState FirmwareUpdateState { get; set; }

    public bool Roaming { get; set; }

    public ChannelDirection ChannelDirection { get; set; }

    public IEnumerable<string> LinkSourceRoles { get; set; } = [];

    public IEnumerable<string> LinkTargetRoles { get; set; } = [];

    public bool IsDevice { get; set; }

    public bool IsChannel { get; set; }
}
