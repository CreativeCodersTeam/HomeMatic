using CreativeCoders.HomeMatic.Core.Devices;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Exporting;

/// <summary>
/// Exports <see cref="ICompleteCcuDevice"/> data to a serialized representation such as JSON.
/// </summary>
[PublicAPI]
public interface IDeviceExporter
{
    /// <summary>
    /// Asynchronously exports a single device to its serialized representation.
    /// </summary>
    /// <param name="device">The device to export.</param>
    /// <param name="options">Optional export options controlling filtering and formatting.</param>
    /// <returns>A task that yields the serialized representation of the device.</returns>
    Task<string> ExportDeviceAsync(ICompleteCcuDevice device, DeviceExportOptions? options = null);

    /// <summary>
    /// Asynchronously exports a sequence of devices to a single serialized representation.
    /// </summary>
    /// <param name="devices">The devices to export.</param>
    /// <param name="options">Optional export options controlling filtering and formatting.</param>
    /// <returns>A task that yields the serialized representation of the devices.</returns>
    Task<string> ExportDevicesAsync(IEnumerable<ICompleteCcuDevice> devices, DeviceExportOptions? options = null);

    /// <summary>
    /// Builds the intermediate <see cref="DeviceExportData"/> representation of a device, applying
    /// the filter rules from <paramref name="options"/> without serializing the result.
    /// </summary>
    /// <param name="device">The device to build the export data for.</param>
    /// <param name="options">Optional export options controlling filtering.</param>
    /// <returns>The filtered <see cref="DeviceExportData"/> for the device.</returns>
    DeviceExportData BuildExportData(ICompleteCcuDevice device, DeviceExportOptions? options = null);
}
