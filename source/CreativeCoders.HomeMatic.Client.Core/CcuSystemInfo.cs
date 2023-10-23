using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Client.Core;

public class CcuSystemInfo
{
    public CcuSystemInfo(string name, HomeMaticDeviceSystem deviceSystem)
    {
        Name = Ensure.NotNull(name);
        DeviceSystem = deviceSystem;
    }

    public string Name { get; }
    
    public HomeMaticDeviceSystem DeviceSystem { get; }
}
