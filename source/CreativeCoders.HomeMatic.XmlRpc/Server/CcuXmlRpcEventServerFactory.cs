using System;
using System.Text;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc.Server.Http;
using CreativeCoders.Net.XmlRpc.Server;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

/// <summary>
/// Creates <see cref="CcuXmlRpcEventServer"/> instances backed by an <see cref="HttpListenerServer"/>.
/// </summary>
/// <remarks>
/// The created XML-RPC server uses <see cref="Encoding.Latin1"/>, the encoding of the CCU interface processes,
/// and owns its <see cref="HttpListenerServer"/>, so disposing the event server also releases the listener.
/// </remarks>
/// <param name="loggerFactory">The logger factory used to create the logger of each event server.</param>
[UsedImplicitly]
public sealed class CcuXmlRpcEventServerFactory(ILoggerFactory loggerFactory) : ICcuXmlRpcEventServerFactory
{
    private readonly ILoggerFactory _loggerFactory = Ensure.NotNull(loggerFactory);

    /// <inheritdoc />
    /// <exception cref="ArgumentException"><paramref name="listenUrl"/> is <see langword="null"/>, empty, or consists only of white-space characters.</exception>
    public ICcuXmlRpcEventServer Create(string listenUrl)
    {
        Ensure.IsNotNullOrWhitespace(listenUrl);

        var xmlRpcServer = new XmlRpcServer(new HttpListenerServer(), true) { Encoding = Encoding.Latin1 };

        return new CcuXmlRpcEventServer(xmlRpcServer, _loggerFactory.CreateLogger<CcuXmlRpcEventServer>())
        {
            ServerUrl = listenUrl
        };
    }
}
