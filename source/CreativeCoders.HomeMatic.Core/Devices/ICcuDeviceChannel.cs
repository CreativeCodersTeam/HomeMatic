using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.HomeMatic.XmlRpc.Links;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Represents a single channel of a HomeMatic device.
/// </summary>
public interface ICcuDeviceChannel : ICcuDeviceBase, ICcuDeviceChannelData
{
    /// <summary>
    /// Asynchronously retrieves all communication links assigned to this channel.
    /// </summary>
    /// <param name="flags">A bitwise combination of <see cref="GetLinksFlags"/> values controlling the level of detail.</param>
    /// <returns>A task that yields a collection of <see cref="Link"/> structures describing each link.</returns>
    Task<IEnumerable<Link>> GetLinksAsync(GetLinksFlags flags = GetLinksFlags.None);

    /// <summary>
    /// Asynchronously retrieves the addresses of all communication partners of this channel.
    /// </summary>
    /// <returns>A task that yields the peer addresses.</returns>
    Task<IEnumerable<string>> GetLinkPeersAsync();

    /// <summary>
    /// Asynchronously creates a communication link from this channel to the specified receiver.
    /// </summary>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <param name="name">An optional name for the link.</param>
    /// <param name="description">An optional description for the link.</param>
    /// <returns>A task that completes when the link has been created.</returns>
    Task AddLinkToAsync(string receiverAddress, string name = "", string description = "");

    /// <summary>
    /// Asynchronously removes the communication link from this channel to the specified receiver.
    /// </summary>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <returns>A task that completes when the link has been removed.</returns>
    Task RemoveLinkToAsync(string receiverAddress);

    /// <summary>
    /// Asynchronously updates the descriptive texts of an existing communication link from this channel.
    /// </summary>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <param name="name">The new name of the link.</param>
    /// <param name="description">The new description of the link.</param>
    /// <returns>A task that completes when the link has been updated.</returns>
    Task SetLinkInfoAsync(string receiverAddress, string name, string description);

    /// <summary>
    /// Asynchronously retrieves the descriptive information of an existing communication link from this channel.
    /// </summary>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <returns>A task that yields a <see cref="LinkInfo"/> instance.</returns>
    Task<LinkInfo> GetLinkInfoAsync(string receiverAddress);

    /// <summary>
    /// Asynchronously activates a link parameter set so that this channel behaves as if it had been
    /// triggered directly by the specified communication partner.
    /// </summary>
    /// <param name="peerAddress">The address of the communication partner whose link parameter set is activated.</param>
    /// <param name="longPress"><see langword="true"/> to activate the parameter set for a long key press; otherwise <see langword="false"/>.</param>
    /// <returns>A task that completes when the parameter set has been activated.</returns>
    Task ActivateLinkParamsetAsync(string peerAddress, bool longPress);
}
