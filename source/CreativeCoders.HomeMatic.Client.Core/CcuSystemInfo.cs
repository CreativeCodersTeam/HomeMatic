using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Client.Core;

public class CcuSystemInfo(string name, HomeMaticDeviceSystem deviceSystem)
{
    public string Name { get; } = Ensure.NotNull(name);

    public HomeMaticDeviceSystem DeviceSystem { get; } = deviceSystem;
}
