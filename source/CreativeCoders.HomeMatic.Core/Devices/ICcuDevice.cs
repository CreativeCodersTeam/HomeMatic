using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

public interface ICcuDevice : ICcuDeviceBase, ICcuDeviceData
{
    IEnumerable<ICcuDeviceChannel> Channels { get; }
}
