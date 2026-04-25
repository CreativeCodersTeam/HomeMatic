using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

/// <summary>
/// Defines an XML-RPC server that receives event callbacks from the HomeMatic CCU.
/// </summary>
/// <remarks>
/// After starting, this server listens for incoming XML-RPC calls from the CCU interface process
/// and dispatches them to all registered <see cref="ICcuEventHandler"/> instances.
/// Register with the CCU by calling <see cref="Client.IHomeMaticXmlRpcApi.InitAsync"/> with the
/// server's URL and an interface identifier.
/// </remarks>
[PublicAPI]
public interface ICcuXmlRpcEventServer
{
    /// <summary>
    /// Starts the XML-RPC event server and begins listening for CCU callbacks.
    /// </summary>
    /// <returns>A task that completes when the server has started.</returns>
    Task StartAsync();

    /// <summary>
    /// Stops the XML-RPC event server.
    /// </summary>
    /// <returns>A task that completes when the server has stopped.</returns>
    Task StopAsync();

    /// <summary>
    /// Registers an event handler to receive CCU-initiated callbacks.
    /// </summary>
    /// <param name="eventHandler">The handler to register for receiving CCU events.</param>
    void RegisterEventHandler(ICcuEventHandler eventHandler);

    /// <summary>
    /// Gets or sets the URL on which this server listens for incoming CCU callbacks.
    /// </summary>
    /// <value>The listen URL (e.g. <c>http://0.0.0.0:5000/</c>). Must be set before calling <see cref="StartAsync"/>.</value>
    string ServerUrl { get; set; }
}
