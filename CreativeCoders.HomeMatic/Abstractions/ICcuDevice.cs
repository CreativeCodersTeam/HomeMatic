namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuDevice : ICcuDeviceChannel
{
    string Name { get; }

    Task<IEnumerable<ICcuDeviceChannel>> GetChannelsAsync();

    Task<ICcuDeviceChannel> GetChannelAsync(string channelAddress);
}
