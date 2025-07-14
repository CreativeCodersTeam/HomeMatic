using CreativeCoders.HomeMatic.Abstractions.Devices;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    Task<ICcuDevice> GetDeviceAsync(string address);

    Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync();

    Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDeviceAsync(string address);
}
