namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICompleteCcuDeviceChannel
{
    ICcuDeviceChannelData ChannelData { get; }

    IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; }
}
