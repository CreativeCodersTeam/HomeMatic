using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

/// <summary>
/// Provides strongly-typed convenience overloads for the link-related methods of
/// <see cref="IHomeMaticXmlRpcApi"/>.
/// </summary>
[PublicAPI]
public static class HomeMaticXmlRpcApiLinkExtensions
{
    /// <summary>
    /// Retrieves all communication links assigned to a logical device or channel using a
    /// strongly-typed <see cref="GetLinksFlags"/> argument.
    /// </summary>
    /// <param name="api">The API instance to invoke.</param>
    /// <param name="address">
    /// The channel or device address. Pass an empty string to retrieve all links of the entire
    /// interface process.
    /// </param>
    /// <param name="flags">A bitwise combination of <see cref="GetLinksFlags"/> values.</param>
    /// <returns>A collection of <see cref="Link"/> structures describing each link.</returns>
    public static Task<IEnumerable<Link>> GetLinksAsync(this IHomeMaticXmlRpcApi api, string address,
        GetLinksFlags flags = GetLinksFlags.None)
    {
        Ensure.NotNull(api);
        Ensure.NotNull(address);

        return api.GetLinksAsync(address, (int) flags);
    }

    /// <summary>
    /// Retrieves the descriptive information of an existing communication link as a
    /// strongly-typed <see cref="LinkInfo"/> instance.
    /// </summary>
    /// <param name="api">The API instance to invoke.</param>
    /// <param name="senderAddress">The address of the sender of the link.</param>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <returns>
    /// A <see cref="LinkInfo"/> instance whose <see cref="LinkInfo.Name"/> and
    /// <see cref="LinkInfo.Description"/> are populated from the XML-RPC response. If the response
    /// contains fewer than two entries, the missing fields default to an empty string.
    /// </returns>
    public static async Task<LinkInfo> GetLinkInfoAsync(this IHomeMaticXmlRpcApi api,
        string senderAddress, string receiverAddress)
    {
        Ensure.NotNull(api);
        Ensure.NotNull(senderAddress);
        Ensure.NotNull(receiverAddress);

        var raw = (await api.GetLinkInfoRawAsync(senderAddress, receiverAddress).ConfigureAwait(false))
            ?.ToArray() ?? [];

        return new LinkInfo
        {
            Name = raw.Length > 0 ? raw[0] ?? string.Empty : string.Empty,
            Description = raw.Length > 1 ? raw[1] ?? string.Empty : string.Empty
        };
    }
}
