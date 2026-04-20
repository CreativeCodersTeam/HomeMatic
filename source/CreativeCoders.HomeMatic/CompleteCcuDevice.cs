using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Represents a HomeMatic device combined with all its parameter-set values and descriptions.
/// </summary>
public class CompleteCcuDevice : ICompleteCcuDevice
{
    /// <inheritdoc />
    public required ICcuDeviceData DeviceData { get; init; }

    /// <inheritdoc />
    public required IEnumerable<ICompleteCcuDeviceChannel> Channels { get; init; }

    /// <inheritdoc />
    public required IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; init; }
}
