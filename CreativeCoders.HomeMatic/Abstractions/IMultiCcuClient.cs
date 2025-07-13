namespace CreativeCoders.HomeMatic.Abstractions;

public interface IMultiCcuClient
{
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();
}
