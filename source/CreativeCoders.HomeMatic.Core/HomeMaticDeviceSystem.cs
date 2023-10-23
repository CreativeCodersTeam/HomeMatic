using System;

namespace CreativeCoders.HomeMatic.Core;

[Flags]
public enum HomeMaticDeviceSystem
{
    HomeMatic = 1,
    HomeMaticIp = 2,
    HomeMaticWired = 4,
    CoupledDevice = 8
}
