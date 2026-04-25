namespace CreativeCoders.HomeMatic.Exporting;

/// <summary>
/// Represents the serialized view of a single HomeMatic channel exported by <see cref="DeviceExporter"/>.
/// </summary>
public class ChannelExportData
{
    /// <summary>
    /// Gets the channel address on the CCU.
    /// </summary>
    /// <value>The channel address, including the channel index suffix.</value>
    public required string Address { get; init; }

    /// <summary>
    /// Gets the device type of the channel as reported by the CCU.
    /// </summary>
    /// <value>The channel's device type string.</value>
    public required string DeviceType { get; init; }

    /// <summary>
    /// Gets the zero-based index of the channel within its parent device.
    /// </summary>
    /// <value>The channel index.</value>
    public required int Index { get; init; }

    /// <summary>
    /// Gets the parameter-set keys available for this channel.
    /// </summary>
    /// <value>The array of parameter-set keys.</value>
    public required string[] ParamSets { get; init; }

    /// <summary>
    /// Gets the parameter-set values of the channel that passed the export filter.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamSetExportData"/> entries.</value>
    public required IEnumerable<ParamSetExportData> ParamSetValues { get; init; }
}
