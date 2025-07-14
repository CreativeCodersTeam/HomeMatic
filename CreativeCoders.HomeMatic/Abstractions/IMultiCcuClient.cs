using CreativeCoders.HomeMatic.Abstractions.Devices;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface IMultiCcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    Task<ICcuDevice> GetDeviceAsync(string address);

    Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync();

    Task<ICompleteCcuDevice> GetCompleteDeviceAsync(string address);
}
