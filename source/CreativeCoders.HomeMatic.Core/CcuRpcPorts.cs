using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

[PublicAPI]
public static class CcuRpcPorts
{
    public const int CoupledDevices = 9292;
        
    public const int HomeMatic = 2001;

    public const int HomeMaticIp = 2010;

    public const int HomeMaticWired = 2000;

    public static int ToPort(this HomeMaticDeviceSystem deviceSystem)
    {
        return deviceSystem switch
        {
            HomeMaticDeviceSystem.HomeMatic => HomeMatic,
            HomeMaticDeviceSystem.HomeMaticIp => HomeMaticIp,
            HomeMaticDeviceSystem.HomeMaticWired => HomeMaticWired,
            HomeMaticDeviceSystem.CoupledDevice => CoupledDevices,
            _ => throw new ArgumentOutOfRangeException(nameof(deviceSystem), deviceSystem, null)
        };
    }
}