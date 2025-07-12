namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    Task<ICcuDevice> GetDeviceAsync(string address);
}
