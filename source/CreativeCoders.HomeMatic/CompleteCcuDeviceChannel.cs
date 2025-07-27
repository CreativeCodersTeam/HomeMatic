using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic;

public class CompleteCcuDeviceChannel : ICompleteCcuDeviceChannel
{
    public required ICcuDeviceChannelData ChannelData { get; init; }

    public required IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; init; }
}
