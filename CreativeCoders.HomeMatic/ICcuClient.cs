namespace CreativeCoders.HomeMatic;

public interface ICcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();
    
    Task<ICcuDevice> GetDeviceAsync(string address);
}

public interface ICcuDevice
{
}
