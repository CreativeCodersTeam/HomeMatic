using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Core;

public interface ICcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    Task<ICcuDevice> GetDeviceAsync(string address);

    Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync();

    Task<ICompleteCcuDevice> GetCompleteDeviceAsync(string address);
}
