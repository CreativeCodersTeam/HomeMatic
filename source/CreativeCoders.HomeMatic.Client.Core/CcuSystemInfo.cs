using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Client.Core;

public class CcuSystemInfo(string name, HomeMaticDeviceSystems deviceSystems)
{
    public string Name { get; } = Ensure.NotNull(name);

    public HomeMaticDeviceSystems DeviceSystems { get; } = deviceSystems;
}
