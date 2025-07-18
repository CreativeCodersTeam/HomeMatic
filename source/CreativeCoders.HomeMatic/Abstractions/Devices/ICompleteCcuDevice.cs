namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICompleteCcuDevice
{
    ICcuDeviceData DeviceData { get; }

    IEnumerable<ICompleteCcuDeviceChannel> Channels { get; }

    IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; }
}
