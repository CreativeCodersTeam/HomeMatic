using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Represents a HomeMatic device including its channels and runtime state.
/// </summary>
public interface ICcuDevice : ICcuDeviceBase, ICcuDeviceData
{
    /// <summary>
    /// Gets the channels that belong to this device.
    /// </summary>
    /// <value>The enumerable of <see cref="ICcuDeviceChannel"/> instances.</value>
    IEnumerable<ICcuDeviceChannel> Channels { get; }
}
