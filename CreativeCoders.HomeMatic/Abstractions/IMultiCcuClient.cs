namespace CreativeCoders.HomeMatic.Abstractions;

public interface IMultiCcuClient
{
    IAsyncEnumerable<ICcuDevice> GetDevicesAsync();
}
