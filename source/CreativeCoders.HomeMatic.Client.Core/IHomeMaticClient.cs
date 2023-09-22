using CreativeCoders.HomeMatic.Client.Core.Devices;

namespace CreativeCoders.HomeMatic.Client.Core;

public interface IHomeMaticClient
{
    Task<CcuDevice> GetDevicesAsync();
}