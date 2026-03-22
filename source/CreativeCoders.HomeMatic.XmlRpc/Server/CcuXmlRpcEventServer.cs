using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.Core.Threading;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Server;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

/// <inheritdoc />
/// <summary>
/// Hosts an XML-RPC server that receives and dispatches event callbacks from the HomeMatic CCU.
/// </summary>
/// <remarks>
/// This server exposes the XML-RPC methods required by the HomeMatic logic layer protocol
/// (<c>event</c>, <c>listDevices</c>, <c>newDevices</c>, <c>deleteDevices</c>, <c>updateDevice</c>).
/// Register with the CCU using <see cref="M:CreativeCoders.HomeMatic.XmlRpc.Client.IHomeMaticXmlRpcApi.InitAsync(System.String,System.String)">Client.IHomeMaticXmlRpcApi.InitAsync</see> after starting.
/// </remarks>
[PublicAPI]
public class CcuXmlRpcEventServer : ICcuXmlRpcEventServer
{
    private readonly IXmlRpcServer _xmlRpcServer;

    private readonly ILogger<CcuXmlRpcEventServer> _logger;

    private readonly IList<ICcuEventHandler> _eventHandlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CcuXmlRpcEventServer"/> class.
    /// </summary>
    /// <param name="xmlRpcServer">The underlying XML-RPC server that handles HTTP transport.</param>
    /// <param name="logger">The logger used to record diagnostic information about received callbacks.</param>
    public CcuXmlRpcEventServer(IXmlRpcServer xmlRpcServer, ILogger<CcuXmlRpcEventServer> logger)
    {
        _xmlRpcServer = Ensure.NotNull(xmlRpcServer);

        _logger = Ensure.NotNull(logger);

        _xmlRpcServer.Methods.RegisterMethods(string.Empty, this);

        _eventHandlers = new ConcurrentList<ICcuEventHandler>();
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No URL has been configured for the HTTP server.</exception>
    public async Task StartAsync()
    {
        if (!string.IsNullOrWhiteSpace(ServerUrl))
        {
            _xmlRpcServer.Urls.Add(ServerUrl);
        }

        if (_xmlRpcServer.Urls.Count == 0)
        {
            throw new InvalidOperationException("No Url for HTTP server specified");
        }

        await _xmlRpcServer.StartAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {
        await _xmlRpcServer.StopAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void RegisterEventHandler(ICcuEventHandler eventHandler)
    {
        _eventHandlers.Add(eventHandler);
    }

    /// <summary>
    /// Handles the <c>event</c> XML-RPC callback from the CCU interface process.
    /// </summary>
    [XmlRpcMethod("event")]
    private async Task<string> Event(string interfaceId, string address, string valueKey, object value)
    {
        _logger.LogDebug(
            "Event from CCU received. InterfaceId = {InterfaceId}; Address = {Address}; ValueKey = {ValueKey}; Value = {Value}",
            interfaceId, address, valueKey, value);

        await _eventHandlers.ForEachAsync(x => x.Event(address, valueKey, value)).ConfigureAwait(false);

        return string.Empty;
    }

    /// <summary>
    /// Handles the <c>listDevices</c> XML-RPC callback. Returns an empty list because
    /// the logic layer does not maintain a separate device registry in this implementation.
    /// </summary>
    [XmlRpcMethod("listDevices")]
    private Task<IEnumerable<XmlRpcValue>> ListDevices(string interfaceId)
    {
        _logger.LogDebug("List devices for InterfaceId = {InterfaceId}", interfaceId);

        return Task.FromResult(Array.Empty<XmlRpcValue>() as IEnumerable<XmlRpcValue>);
    }

    /// <summary>
    /// Handles the <c>newDevices</c> XML-RPC callback from the CCU interface process.
    /// </summary>
    [XmlRpcMethod("newDevices")]
    private async Task<string> NewDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
    {
        _logger.LogDebug(
            "CCU notifies about new Devices for InterfaceId = {InterfaceId}. New device count: {DeviceCount}",
            interfaceId, deviceDescriptions.Length);

        deviceDescriptions.ForEach(deviceDescription =>
            _logger.LogTrace("New device: Address = {Address}", deviceDescription.Address));

        await _eventHandlers.ForEachAsync(x => x.NewDevices(deviceDescriptions)).ConfigureAwait(false);

        return string.Empty;
    }

    /// <summary>
    /// Handles the <c>deleteDevices</c> XML-RPC callback from the CCU interface process.
    /// </summary>
    [XmlRpcMethod("deleteDevices")]
    private async Task<string> DeleteDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
    {
        _logger.LogDebug(
            "CCU notifies about deleted devices for InterfaceId = {InterfaceId}. Deleted device count: {DeviceCount}",
            interfaceId, deviceDescriptions.Length);

        deviceDescriptions.ForEach(deviceDescription =>
            _logger.LogTrace("Deleted device: Address = {Address}", deviceDescription.Address));

        await _eventHandlers.ForEachAsync(x => x.DeleteDevices(deviceDescriptions)).ConfigureAwait(false);

        return string.Empty;
    }

    /// <summary>
    /// Handles the <c>updateDevice</c> XML-RPC callback from the CCU interface process.
    /// </summary>
    [XmlRpcMethod("updateDevice")]
    private async Task<string> UpdateDevice(string interfaceId, string address, int hint)
    {
        _logger.LogDebug(
            "CCU device is updated. InterfaceId = {InterfaceId}; Address = {Address}; Hint = {Hint}",
            interfaceId, address, hint);

        await _eventHandlers.ForEachAsync(x => x.UpdateDevice(address, hint)).ConfigureAwait(false);

        return string.Empty;
    }

    /// <inheritdoc/>
    public string ServerUrl { get; set; } = string.Empty;
}
