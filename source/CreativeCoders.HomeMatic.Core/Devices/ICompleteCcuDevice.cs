using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

public interface ICompleteCcuDevice
{
    ICcuDeviceData DeviceData { get; }

    IEnumerable<ICompleteCcuDeviceChannel> Channels { get; }

    IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; }
}
