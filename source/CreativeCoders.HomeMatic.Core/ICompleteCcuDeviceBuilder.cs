using System.Threading.Tasks;
using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Core;

public interface ICompleteCcuDeviceBuilder
{
    Task<ICompleteCcuDevice> BuildAsync(ICcuDevice device);
}
