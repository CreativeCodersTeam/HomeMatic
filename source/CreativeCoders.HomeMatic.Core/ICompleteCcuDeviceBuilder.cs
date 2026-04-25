using System.Threading.Tasks;
using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Builds <see cref="ICompleteCcuDevice"/> instances from basic <see cref="ICcuDevice"/> data by loading parameter descriptions.
/// </summary>
public interface ICompleteCcuDeviceBuilder
{
    /// <summary>
    /// Asynchronously builds a complete device representation for the specified device.
    /// </summary>
    /// <param name="device">The base device to augment with parameter descriptions.</param>
    /// <returns>A task that yields the completed <see cref="ICompleteCcuDevice"/>.</returns>
    Task<ICompleteCcuDevice> BuildAsync(ICcuDevice device);
}
