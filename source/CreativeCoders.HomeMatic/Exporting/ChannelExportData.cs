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
    /// Gets the exported parameter-set keys of this channel.
    /// </summary>
    /// <value>The array of parameter-set keys.</value>
    /// <remarks>
    /// The keys are filtered by the export options: a ParamSet excluded by
    /// <see cref="DeviceExportOptions.ParamSetWhitelist"/> or by
    /// <see cref="DeviceExportOptions.SkipServiceParamSet"/> is absent here as well. Without export options the
    /// channel reports all of its parameter-set keys.
    /// </remarks>
    public required string[] ParamSets { get; init; }

    /// <summary>
    /// Gets the parameter-set values of the channel that passed the export filter.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamSetExportData"/> entries.</value>
    public required IEnumerable<ParamSetExportData> ParamSetValues { get; init; }

    /// <summary>
    /// Gets the communication links of the channel that passed the export filter.
    /// </summary>
    /// <value>
    /// The enumerable of <see cref="LinkExportData"/> entries, or <see langword="null"/> when link
    /// export is disabled. <see langword="null"/> values are omitted from the JSON output.
    /// </value>
    public IEnumerable<LinkExportData>? Links { get; init; }
}
