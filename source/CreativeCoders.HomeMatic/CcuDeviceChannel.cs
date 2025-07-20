using CreativeCoders.HomeMatic.Abstractions.Devices;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class CcuDeviceChannel(IHomeMaticXmlRpcApi api) : CcuDeviceBase(api), ICcuDeviceChannel
{
    public required int Index { get; init; }

    public required string Group { get; init; }

    public required ChannelDirection ChannelDirection { get; init; }
}
