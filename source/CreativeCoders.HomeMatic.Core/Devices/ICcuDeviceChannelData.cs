using CreativeCoders.HomeMatic.XmlRpc.Devices;

namespace CreativeCoders.HomeMatic.Core.Devices;

public interface ICcuDeviceChannelData : ICcuDeviceBaseData
{
    int Index { get; }

    string Group { get; }

    ChannelDirection ChannelDirection { get; }
}
