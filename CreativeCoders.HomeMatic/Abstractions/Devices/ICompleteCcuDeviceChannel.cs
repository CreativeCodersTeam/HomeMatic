namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICompleteCcuDeviceChannel
{
    ICcuDeviceChannelData ChannelData { get; }

    IEnumerable<IParamSetValuesWithDescriptions> ParamSetValues { get; }
}
