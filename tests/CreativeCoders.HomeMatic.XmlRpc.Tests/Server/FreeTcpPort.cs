using System.Net;
using System.Net.Sockets;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Server;

/// <summary>
/// Provides free loopback TCP ports and matching HttpListener prefixes for loopback tests.
/// </summary>
internal static class FreeTcpPort
{
    public static int Get()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();

        try
        {
            return ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        }
        finally
        {
            tcpListener.Stop();
        }
    }

    public static string GetLoopbackPrefix()
    {
        return $"http://127.0.0.1:{Get()}/";
    }
}
