using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

public interface ICompleteCcuDeviceChannel
{
    ICcuDeviceChannelData ChannelData { get; }

    IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; }
}
