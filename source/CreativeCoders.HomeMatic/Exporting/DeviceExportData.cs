namespace CreativeCoders.HomeMatic.Exporting;

/// <summary>
/// Represents the serialized view of a HomeMatic device exported by <see cref="DeviceExporter"/>.
/// </summary>
public class DeviceExportData
{
    /// <summary>
    /// Gets the human-readable name of the device.
    /// </summary>
    /// <value>The device name.</value>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the device address on the CCU.
    /// </summary>
    /// <value>The device address.</value>
    public required string Address { get; init; }

    /// <summary>
    /// Gets the device type as reported by the CCU.
    /// </summary>
    /// <value>The device type string.</value>
    public required string DeviceType { get; init; }

    /// <summary>
    /// Gets the parameter-set keys available for this device.
    /// </summary>
    /// <value>The array of parameter-set keys.</value>
    public required string[] ParamSetKeys { get; init; }

    /// <summary>
    /// Gets the currently installed firmware version of the device.
    /// </summary>
    /// <value>The firmware version string.</value>
    public required string FirmwareVersion { get; init; }

    /// <summary>
    /// Gets the display name of the CCU that owns the device.
    /// </summary>
    /// <value>The CCU display name.</value>
    public required string Ccu { get; init; }

    /// <summary>
    /// Gets the parameter-set values of the device that passed the export filter.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamSetExportData"/> entries.</value>
    public required IEnumerable<ParamSetExportData> ParamSetValues { get; init; }

    /// <summary>
    /// Gets the exported channels of the device.
    /// </summary>
    /// <value>The enumerable of <see cref="ChannelExportData"/> entries.</value>
    public required IEnumerable<ChannelExportData> Channels { get; init; }
}
