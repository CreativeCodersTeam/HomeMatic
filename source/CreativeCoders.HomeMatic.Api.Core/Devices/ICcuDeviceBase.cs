using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core.Devices;

[PublicAPI]
public interface ICcuDeviceBase
{
    string Address { get; }
        
    string DeviceType { get; }
        
    int Version { get; }
        
    bool IsAesActive { get; }
        
    string[] ParamSets { get; }

    bool Roaming { get; }
        
    bool IsDevice { get; }
        
    bool IsChannel { get; }
}