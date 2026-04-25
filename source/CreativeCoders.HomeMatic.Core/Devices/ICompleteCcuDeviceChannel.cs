using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Represents a channel combined with all its parameter-set values and descriptions.
/// </summary>
public interface ICompleteCcuDeviceChannel
{
    /// <summary>
    /// Gets the channel-level data.
    /// </summary>
    /// <value>The <see cref="ICcuDeviceChannelData"/> for this channel.</value>
    ICcuDeviceChannelData ChannelData { get; }

    /// <summary>
    /// Gets the parameter-set values and descriptions for the channel.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamSetValuesWithDescriptions"/> groups.</value>
    IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; }
}
