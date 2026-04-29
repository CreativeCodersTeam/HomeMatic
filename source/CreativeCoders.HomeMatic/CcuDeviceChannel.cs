using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Links;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Represents a single channel of a HomeMatic device.
/// </summary>
/// <param name="api">The XML-RPC API used to query parameter-set values and descriptions and to manage communication links.</param>
public class CcuDeviceChannel(IHomeMaticXmlRpcApi api) : CcuDeviceBase(api), ICcuDeviceChannel
{
    /// <inheritdoc />
    public required int Index { get; init; }

    /// <inheritdoc />
    public required string Group { get; init; }

    /// <inheritdoc />
    public required ChannelDirection ChannelDirection { get; init; }

    /// <inheritdoc />
    public Task<IEnumerable<Link>> GetLinksAsync(GetLinksFlags flags = GetLinksFlags.None)
    {
        return Api.GetLinksAsync(Uri.Address, flags);
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> GetLinkPeersAsync()
    {
        return Api.GetLinkPeersAsync(Uri.Address);
    }

    /// <inheritdoc />
    public Task AddLinkToAsync(string receiverAddress, string name = "", string description = "")
    {
        Ensure.IsNotNullOrWhitespace(receiverAddress);
        Ensure.NotNull(name);
        Ensure.NotNull(description);

        return Api.AddLinkAsync(Uri.Address, receiverAddress, name, description);
    }

    /// <inheritdoc />
    public Task RemoveLinkToAsync(string receiverAddress)
    {
        Ensure.IsNotNullOrWhitespace(receiverAddress);

        return Api.RemoveLinkAsync(Uri.Address, receiverAddress);
    }

    /// <inheritdoc />
    public Task SetLinkInfoAsync(string receiverAddress, string name, string description)
    {
        Ensure.IsNotNullOrWhitespace(receiverAddress);
        Ensure.NotNull(name);
        Ensure.NotNull(description);

        return Api.SetLinkInfoAsync(Uri.Address, receiverAddress, name, description);
    }

    /// <inheritdoc />
    public Task<LinkInfo> GetLinkInfoAsync(string receiverAddress)
    {
        Ensure.IsNotNullOrWhitespace(receiverAddress);

        return Api.GetLinkInfoAsync(Uri.Address, receiverAddress);
    }

    /// <inheritdoc />
    public Task ActivateLinkParamsetAsync(string peerAddress, bool longPress)
    {
        Ensure.IsNotNullOrWhitespace(peerAddress);

        return Api.ActivateLinkParamsetAsync(Uri.Address, peerAddress, longPress);
    }
}
