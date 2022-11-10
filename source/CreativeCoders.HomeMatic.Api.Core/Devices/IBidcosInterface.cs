using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core.Devices;

[PublicAPI]
public interface IBidcosInterfaceInfo
{
    string Address { get; }
        
    string Description { get; }
        
    bool IsConnected { get; }
        
    bool IsDefault { get; }
}