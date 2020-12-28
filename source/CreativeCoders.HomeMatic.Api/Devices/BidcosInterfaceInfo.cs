using CreativeCoders.HomeMatic.Api.Core.Devices;

namespace CreativeCoders.HomeMatic.Api.Devices
{
    public class BidcosInterfaceInfo : IBidcosInterfaceInfo
    {
        public string Address { get; init; }
        
        public string Description { get; init; }
        
        public bool IsConnected { get; init; }
        
        public bool IsDefault { get; init; }
    }
}