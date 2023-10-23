using CreativeCoders.HomeMatic.Client.Core.Devices;

namespace CreativeCoders.HomeMatic.Client.Core;

public interface IHomeMaticClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();
    
    Task<CcuDeviceDescription> GetDeviceDescriptionAsync(string address); 
}