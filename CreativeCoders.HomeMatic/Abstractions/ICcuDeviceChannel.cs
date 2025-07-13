using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuDeviceChannel : ICcuDeviceBase
{
    int Index { get; }

    string Group { get; }

    ChannelDirection ChannelDirection { get; }
}
