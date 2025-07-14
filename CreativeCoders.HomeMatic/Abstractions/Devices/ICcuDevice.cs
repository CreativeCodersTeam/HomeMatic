namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICcuDevice : ICcuDeviceBase, ICcuDeviceData
{
    IEnumerable<ICcuDeviceChannel> Channels { get; }
}
