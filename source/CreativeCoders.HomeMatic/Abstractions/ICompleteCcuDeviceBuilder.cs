using CreativeCoders.HomeMatic.Abstractions.Devices;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICompleteCcuDeviceBuilder
{
    Task<ICompleteCcuDevice> BuildAsync(ICcuDevice device);
}
