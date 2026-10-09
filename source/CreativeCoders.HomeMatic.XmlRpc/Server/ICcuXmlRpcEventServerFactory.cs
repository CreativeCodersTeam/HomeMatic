using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

/// <summary>
/// Defines a factory that creates ready-to-start <see cref="ICcuXmlRpcEventServer"/> instances.
/// </summary>
[PublicAPI]
public interface ICcuXmlRpcEventServerFactory
{
    /// <summary>
    /// Creates a new CCU event server that listens on the specified URL prefix.
    /// </summary>
    /// <param name="listenUrl">
    /// The <see cref="System.Net.HttpListener"/> prefix to listen on, for example <c>http://+:53817/</c>.
    /// The prefix must end with <c>/</c>.
    /// </param>
    /// <returns>A new, not yet started event server whose <see cref="ICcuXmlRpcEventServer.ServerUrl"/> is <paramref name="listenUrl"/>.</returns>
    /// <remarks>
    /// The caller owns the returned server and must dispose it with
    /// <see cref="System.IAsyncDisposable.DisposeAsync"/> to release the HTTP listener.
    /// </remarks>
    ICcuXmlRpcEventServer Create(string listenUrl);
}
