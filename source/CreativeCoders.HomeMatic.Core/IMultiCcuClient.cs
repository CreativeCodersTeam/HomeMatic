using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.HomeMatic.Core.Devices;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Provides unified access to devices across multiple HomeMatic CCUs.
/// </summary>
[PublicAPI]
public interface IMultiCcuClient
{
    /// <summary>
    /// Asynchronously retrieves all devices from every configured CCU.
    /// </summary>
    /// <returns>A task that yields an enumerable of <see cref="ICcuDevice"/> instances.</returns>
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    /// <summary>
    /// Asynchronously retrieves a single device by its address across all configured CCUs.
    /// </summary>
    /// <param name="address">The device address.</param>
    /// <returns>A task that yields the matching <see cref="ICcuDevice"/>.</returns>
    Task<ICcuDevice> GetDeviceAsync(string address);

    /// <summary>
    /// Asynchronously retrieves all devices including their parameter descriptions from every configured CCU.
    /// </summary>
    /// <returns>A task that yields an enumerable of <see cref="ICompleteCcuDevice"/> instances.</returns>
    Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync();

    /// <summary>
    /// Asynchronously retrieves a single device including its parameter descriptions across all configured CCUs.
    /// </summary>
    /// <param name="address">The device address.</param>
    /// <returns>A task that yields the matching <see cref="ICompleteCcuDevice"/>.</returns>
    Task<ICompleteCcuDevice> GetCompleteDeviceAsync(string address);
}
