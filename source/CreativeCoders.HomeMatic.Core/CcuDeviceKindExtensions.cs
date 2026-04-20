using System;
using CreativeCoders.HomeMatic.XmlRpc;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

[PublicAPI]
public static class CcuDeviceKindExtensions
{
    public static int ToPort(this CcuDeviceKind deviceKind)
    {
        return deviceKind switch
        {
            CcuDeviceKind.HomeMatic => CcuRpcPorts.HomeMatic,
            CcuDeviceKind.HomeMaticIp => CcuRpcPorts.HomeMaticIp,
            CcuDeviceKind.HomeMaticWired => CcuRpcPorts.HomeMaticWired,
            CcuDeviceKind.Coupled => CcuRpcPorts.CoupledDevices,
            _ => throw new ArgumentOutOfRangeException(nameof(deviceKind), deviceKind, null)
        };
    }
}
