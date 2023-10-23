using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDevice : CcuDeviceBase, ICcuDevice
{
    public CcuDevice(CcuSystemInfo ccuSystemInfo, string? name, DeviceDescription deviceDescription)
    {
        CcuSystem = ccuSystemInfo;
        
        Name = name ?? deviceDescription.Address;
        
        Address = deviceDescription.Address;
        DeviceType = deviceDescription.DeviceType;
        ParamSets = deviceDescription.ParamSets;
    }
    
    public CcuSystemInfo? CcuSystem { get; set; }
    
    public string Name { get; }

    public string Address { get; }

    public string DeviceType { get; }
    
    public string[] ParamSets { get; }
}
