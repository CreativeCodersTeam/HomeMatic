using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Links;

namespace CreativeCoders.HomeMatic;

/// <inheritdoc />
/// <summary>
/// Represents a channel combined with all its parameter-set values and descriptions.
/// </summary>
public class CompleteCcuDeviceChannel : ICompleteCcuDeviceChannel
{
    /// <inheritdoc />
    public required ICcuDeviceChannel ChannelData { get; init; }

    /// <inheritdoc />
    public required IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; init; }

    /// <inheritdoc />
    public IEnumerable<Link> Links { get; init; } = [];
}
