using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Represents a HomeMatic device combined with all its parameter-set values and descriptions.
/// </summary>
public interface ICompleteCcuDevice
{
    /// <summary>
    /// Gets the device-level data.
    /// </summary>
    /// <value>The <see cref="ICcuDeviceData"/> for this device.</value>
    ICcuDeviceData DeviceData { get; }

    /// <summary>
    /// Gets the channels of the device with their parameter-set values and descriptions.
    /// </summary>
    /// <value>The enumerable of <see cref="ICompleteCcuDeviceChannel"/> instances.</value>
    IEnumerable<ICompleteCcuDeviceChannel> Channels { get; }

    /// <summary>
    /// Gets the parameter-set values and descriptions for the device itself.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamSetValuesWithDescriptions"/> groups.</value>
    IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; }
}
