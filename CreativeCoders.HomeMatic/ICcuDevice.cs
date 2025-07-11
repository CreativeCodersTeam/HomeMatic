namespace CreativeCoders.HomeMatic;

public interface ICcuDevice : ICcuDeviceChannel
{
    string Name { get; }
    
    Task<IEnumerable<ICcuDeviceChannel>> GetChannelsAsync();
    
    Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress);
}

public class CcuDevice : CcuDeviceChannel, ICcuDevice
{
    public string Name { get; set; } = string.Empty;
    
    public Task<IEnumerable<ICcuDeviceChannel>> GetChannelsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress)
    {
        throw new NotImplementedException();
    }
}
