using CreativeCoders.HomeMatic.Abstractions.Devices;

namespace CreativeCoders.HomeMatic;

public class CompleteCcuDeviceChannel : ICompleteCcuDeviceChannel
{
    public required ICcuDeviceChannelData ChannelData { get; init; }

    public required IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; init; }
}
