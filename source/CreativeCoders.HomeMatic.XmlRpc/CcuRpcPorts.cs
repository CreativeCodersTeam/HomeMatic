using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Provides the well-known TCP port numbers used by the CCU XML-RPC interfaces.
/// </summary>
[PublicAPI]
public static class CcuRpcPorts
{
    /// <summary>
    /// The XML-RPC port for coupled (multi-system) devices.
    /// </summary>
    public const int CoupledDevices = 9292;

    /// <summary>
    /// The XML-RPC port for the classic BidCoS-RF HomeMatic system.
    /// </summary>
    public const int HomeMatic = 2001;

    /// <summary>
    /// The XML-RPC port for the HomeMatic IP system.
    /// </summary>
    public const int HomeMaticIp = 2010;

    /// <summary>
    /// The XML-RPC port for the HomeMatic Wired (RS485) system.
    /// </summary>
    public const int HomeMaticWired = 2000;

    /// <summary>
    /// Returns the XML-RPC port number used by the CCU for the specified device kind.
    /// </summary>
    /// <param name="deviceKind">One of the enumeration values that specifies the device kind.</param>
    /// <returns>The TCP port number used by the CCU XML-RPC endpoint for <paramref name="deviceKind"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="deviceKind"/> is not a defined value of <see cref="CcuDeviceKind"/>.</exception>
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
