using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICcuDeviceChannelData : ICcuDeviceBaseData
{
    int Index { get; }

    string Group { get; }

    ChannelDirection ChannelDirection { get; }
}
