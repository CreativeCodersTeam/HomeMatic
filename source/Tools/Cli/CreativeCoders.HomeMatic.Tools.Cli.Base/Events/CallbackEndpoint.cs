using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Events;

/// <summary>
/// Represents the local HTTP endpoint on which a CCU delivers its event callbacks.
/// </summary>
/// <param name="Host">The host name or IP address the CCU uses to reach this machine.</param>
/// <param name="Port">The local TCP port of the callback endpoint.</param>
public sealed record CallbackEndpoint(string Host, int Port)
{
    /// <summary>
    /// Gets the HTTP listener prefix that binds the endpoint to all local addresses.
    /// </summary>
    /// <value>The prefix in the form <c>http://+:&lt;Port&gt;/</c>.</value>
    [SuppressMessage("csharpsquid", "S5332:Using clear-text protocols is security-sensitive",
        Justification = "The CCU delivers XML-RPC event callbacks over plain HTTP in the local network.")]
    public string ListenPrefix => $"http://+:{Port}/";

    /// <summary>
    /// Gets the URL the CCU calls back, as passed to the CCU <c>init</c> method.
    /// </summary>
    /// <value>
    /// The URL in the form <c>http://&lt;Host&gt;:&lt;Port&gt;/</c>. An IPv6 address in <see cref="Host"/> is
    /// enclosed in square brackets, for example <c>http://[fe80::1]:&lt;Port&gt;/</c>, unless it is already enclosed.
    /// </value>
    [SuppressMessage("csharpsquid", "S5332:Using clear-text protocols is security-sensitive",
        Justification = "The CCU delivers XML-RPC event callbacks over plain HTTP in the local network.")]
    public string CallbackUrl => $"http://{UrlHost}:{Port}/";

    private string UrlHost =>
        !Host.StartsWith('[')
        && IPAddress.TryParse(Host, out var address)
        && address.AddressFamily == AddressFamily.InterNetworkV6
            ? $"[{Host}]"
            : Host;
}
