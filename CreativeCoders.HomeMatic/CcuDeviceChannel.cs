using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic;

public class CcuDeviceChannel : ICcuDeviceChannel
{
    public required CcuDeviceUri Uri { get; init; }
    public required string DeviceType { get; init; }

    public required bool IsAesActive { get; init; }

    public required string Interface { get; init; }

    public required int Version { get; init; }

    public required bool Roaming { get; init; }

    public required string[] ParamSets { get; init; }

    public required int Index { get; init; }

    public required string Group { get; init; }

    public required ChannelDirection ChannelDirection { get; init; }
}
