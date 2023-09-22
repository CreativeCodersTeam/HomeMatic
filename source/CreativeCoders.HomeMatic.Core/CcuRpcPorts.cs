using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

[PublicAPI]
public static class CcuRpcPorts
{
    public const int CoupledDevices = 9292;
        
    public const int HomeMatic = 2001;

    public const int HomeMaticIp = 2010;

    public const int HomeMaticWired = 2000;
}