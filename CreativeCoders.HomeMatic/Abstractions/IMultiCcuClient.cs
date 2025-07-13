namespace CreativeCoders.HomeMatic.Abstractions;

public interface IMultiCcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    Task<ICcuDevice> GetDeviceAsync(string address);
}
