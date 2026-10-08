using System.Net;
using System.Net.Sockets;
using CreativeCoders.Core;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Events;

/// <summary>
/// Resolves the callback endpoint from the given options, the network route to the CCU and a free local port.
/// </summary>
/// <remarks>
/// The automatic host is the local IPv4 address of a UDP socket connected to the first IPv4 address of the CCU;
/// connecting a UDP socket sends no packet. A CCU without an IPv4 address needs an explicit callback host. The automatic port is taken from a <see cref="TcpListener"/> that is started on port <c>0</c>
/// and stopped right away, so another process may take the port before it is bound again.
/// </remarks>
[UsedImplicitly]
public sealed class CallbackEndpointResolver : ICallbackEndpointResolver
{
    /// <inheritdoc />
    public CallbackEndpoint Resolve(Uri ccuUrl, string? callbackHost, int callbackPort)
    {
        Ensure.NotNull(ccuUrl);
        ArgumentOutOfRangeException.ThrowIfNegative(callbackPort);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(callbackPort, IPEndPoint.MaxPort);

        var host = string.IsNullOrWhiteSpace(callbackHost)
            ? GetLocalAddressRoutedTo(ccuUrl)
            : callbackHost;

        var port = callbackPort == 0
            ? GetFreePort()
            : callbackPort;

        return new CallbackEndpoint(host, port);
    }

    private static string GetLocalAddressRoutedTo(Uri ccuUrl)
    {
        var ccuAddress = Dns.GetHostAddresses(ccuUrl.DnsSafeHost)
                             .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
                         ?? throw new SocketException((int)SocketError.AddressFamilyNotSupported);

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        socket.Connect(ccuAddress, ccuUrl.Port);

        return ((IPEndPoint)socket.LocalEndPoint!).Address.ToString();
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Any, 0);

        try
        {
            listener.Start();

            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        catch (SocketException ex)
        {
            throw new CallbackPortAllocationException(ex.Message, ex);
        }
        finally
        {
            listener.Stop();
        }
    }
}
