using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDevice : CcuDeviceBase
{
    public CcuDevice(CcuSystemInfo ccuSystemInfo, string? name, DeviceDescription deviceDescription)
    {
        CcuSystem = ccuSystemInfo;
        
        Name = name ?? deviceDescription.Address;
        
        Address = deviceDescription.Address;
        DeviceType = deviceDescription.DeviceType;
    }
    
    public CcuSystemInfo? CcuSystem { get; set; }
    
    public string? Name { get; init; }

    public string? Address { get; set; }

    public string? DeviceType { get; set; }
}
