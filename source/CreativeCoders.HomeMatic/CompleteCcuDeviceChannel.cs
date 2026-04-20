using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Represents a channel combined with all its parameter-set values and descriptions.
/// </summary>
public class CompleteCcuDeviceChannel : ICompleteCcuDeviceChannel
{
    /// <inheritdoc />
    public required ICcuDeviceChannelData ChannelData { get; init; }

    /// <inheritdoc />
    public required IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; init; }
}
