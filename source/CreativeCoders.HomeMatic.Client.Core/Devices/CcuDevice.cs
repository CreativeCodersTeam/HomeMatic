using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDevice(CcuSystemInfo ccuSystemInfo, string? name, DeviceDescription deviceDescription)
    : CcuDeviceBase, ICcuDevice
{
    public CcuSystemInfo? CcuSystem { get; set; } = ccuSystemInfo;

    public string Name { get; } = name ?? deviceDescription.Address;

    public string Address { get; } = deviceDescription.Address;

    public string DeviceType { get; } = deviceDescription.DeviceType;

    public string[] ParamSets { get; } = deviceDescription.ParamSets;
}
