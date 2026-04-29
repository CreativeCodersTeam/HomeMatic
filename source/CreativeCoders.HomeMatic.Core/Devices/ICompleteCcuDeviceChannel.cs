using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc.Links;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Represents a channel combined with all its parameter-set values and descriptions.
/// </summary>
public interface ICompleteCcuDeviceChannel
{
    /// <summary>
    /// Gets the channel and its operations.
    /// </summary>
    /// <value>The <see cref="ICcuDeviceChannel"/> for this channel.</value>
    ICcuDeviceChannel ChannelData { get; }

    /// <summary>
    /// Gets the parameter-set values and descriptions for the channel.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamSetValuesWithDescriptions"/> groups.</value>
    IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; }

    /// <summary>
    /// Gets the communication links of the channel that were fetched during snapshot creation.
    /// </summary>
    /// <value>
    /// The collection of <see cref="Link"/> structures. Empty when the snapshot was built without
    /// link fetching enabled.
    /// </value>
    IEnumerable<Link> Links { get; }
}
