using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic;

public class CompleteCcuDevice : ICompleteCcuDevice
{
    public required ICcuDeviceData DeviceData { get; init; }

    public required IEnumerable<ICompleteCcuDeviceChannel> Channels { get; init; }

    public required IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; init; }
}
