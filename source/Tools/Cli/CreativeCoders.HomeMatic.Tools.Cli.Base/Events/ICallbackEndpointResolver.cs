namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Events;

/// <summary>
/// Defines a resolver that determines the local endpoint on which a CCU delivers its event callbacks.
/// </summary>
public interface ICallbackEndpointResolver
{
    /// <summary>
    /// Determines the callback host and port for the specified CCU.
    /// </summary>
    /// <param name="ccuUrl">The base URL of the CCU.</param>
    /// <param name="callbackHost">
    /// The host name or IP address the CCU uses to reach this machine, or <see langword="null"/> or white space to
    /// use the local address the operating system routes to the CCU.
    /// </param>
    /// <param name="callbackPort">The local TCP port, or <c>0</c> to pick a free port automatically.</param>
    /// <returns>The resolved callback endpoint.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ccuUrl"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="callbackPort"/> is less than 0 or greater than 65535.</exception>
    /// <exception cref="System.Net.Sockets.SocketException">
    /// The CCU host cannot be resolved to an IPv4 address, or the local address routed to it cannot be determined.
    /// </exception>
    /// <exception cref="CallbackPortAllocationException">
    /// <paramref name="callbackPort"/> is <c>0</c> and no free local TCP port can be allocated.
    /// </exception>
    CallbackEndpoint Resolve(Uri ccuUrl, string? callbackHost, int callbackPort);
}
