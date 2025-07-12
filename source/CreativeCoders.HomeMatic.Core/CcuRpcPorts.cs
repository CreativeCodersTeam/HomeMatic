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

    public static int ToPort(this HomeMaticDeviceSystems deviceSystems)
    {
        return deviceSystems switch
        {
            HomeMaticDeviceSystems.HomeMatic => HomeMatic,
            HomeMaticDeviceSystems.HomeMaticIp => HomeMaticIp,
            HomeMaticDeviceSystems.HomeMaticWired => HomeMaticWired,
            HomeMaticDeviceSystems.CoupledDevice => CoupledDevices,
            _ => throw new ArgumentOutOfRangeException(nameof(deviceSystems), deviceSystems, null)
        };
    }

    public static int ToPort(this CcuDeviceKind deviceKind)
    {
        return deviceKind switch
        {
            CcuDeviceKind.HomeMatic => HomeMatic,
            CcuDeviceKind.HomeMaticIp => HomeMaticIp,
            CcuDeviceKind.HomeMaticWired => HomeMaticWired,
            CcuDeviceKind.Coupled => CoupledDevices,
            _ => throw new ArgumentOutOfRangeException(nameof(deviceKind), deviceKind, null)
        };
    }
}
