using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class CcuDevice(IHomeMaticXmlRpcApi api) : CcuDeviceChannel, ICcuDevice
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
