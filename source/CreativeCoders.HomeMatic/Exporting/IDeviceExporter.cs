using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Exporting;

public interface IDeviceExporter
{
    Task<string> ExportDeviceAsync(ICompleteCcuDevice device, DeviceExportOptions? options = null);

    Task<string> ExportDevicesAsync(IEnumerable<ICompleteCcuDevice> devices, DeviceExportOptions? options = null);

    DeviceExportData BuildExportData(ICompleteCcuDevice device, DeviceExportOptions? options = null);
}
