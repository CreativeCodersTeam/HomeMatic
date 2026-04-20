using System;

namespace CreativeCoders.HomeMatic.XmlRpc;

[Flags]
public enum HomeMaticDeviceSystems
{
    HomeMatic = 1,
    HomeMaticIp = 2,
    HomeMaticWired = 4,
    CoupledDevice = 8
}
