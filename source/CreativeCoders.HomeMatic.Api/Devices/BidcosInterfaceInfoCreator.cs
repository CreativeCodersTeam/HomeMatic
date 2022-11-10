using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Api.Devices;

public static class BidcosInterfaceInfoCreator
{
    public static BidcosInterfaceInfo Create(BidcosInterface bidcosInterface)
    {
        return new BidcosInterfaceInfo
        {
            Address = bidcosInterface.Address,
            Description = bidcosInterface.Description,
            IsConnected = bidcosInterface.IsConnected,
            IsDefault = bidcosInterface.IsDefault
        };
    }
}