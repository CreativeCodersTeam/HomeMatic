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

[PublicAPI]
public class CcuXmlRpcEventServer : ICcuXmlRpcEventServer
{
    private readonly IXmlRpcServer _xmlRpcServer;
    private readonly ILogger<CcuXmlRpcEventServer> _logger;

    private IList<ICcuEventHandler> _eventHandlers;

    public CcuXmlRpcEventServer(IXmlRpcServer xmlRpcServer, ILogger<CcuXmlRpcEventServer> logger)
    {
        _xmlRpcServer = xmlRpcServer;
        _logger = Ensure.NotNull(logger, nameof(logger));

        _xmlRpcServer.Methods.RegisterMethods(string.Empty, this);
            
        _eventHandlers = new ConcurrentList<ICcuEventHandler>();
    }

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

    public async Task StopAsync()
    {
        await _xmlRpcServer.StopAsync().ConfigureAwait(false);
    }

    public void RegisterEventHandler(ICcuEventHandler eventHandler)
    {
        _eventHandlers.Add(eventHandler);
    }

    [XmlRpcMethod("event")]
    private async Task<string> Event(string interfaceId, string address, string valueKey, object value)
    {
        _logger.LogDebug(
            "Event from CCU received. InterfaceId = {InterfaceId}; Address = {Address}; ValueKey = {ValueKey}; Value = {Value}",
            interfaceId, address, valueKey, value);

        await _eventHandlers.ForEachAsync(x => x.Event(address, valueKey, value));

        return string.Empty;
    }

    [XmlRpcMethod("listDevices")]
    private Task<IEnumerable<XmlRpcValue>> ListDevices(string interfaceId)
    {
        _logger.LogDebug("List devices for InterfaceId = {InterfaceId}", interfaceId);
            
        return Task.FromResult(Array.Empty<XmlRpcValue>() as IEnumerable<XmlRpcValue>);
    }

    [XmlRpcMethod("newDevices")]
    private async Task<string> NewDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
    {
        _logger.LogDebug(
            "CCU notifies about new Devices for InterfaceId = {InterfaceId}. New device count: {DeviceDescriptionsCount}",
            interfaceId, deviceDescriptions.Length);
            
        deviceDescriptions
            .ForEach(deviceDescription =>
                _logger.LogTrace("New device: Address = {DeviceDescriptionAddress}", deviceDescription.Address));

        await _eventHandlers.ForEachAsync(x => x.NewDevices(deviceDescriptions));
            
        return string.Empty;
    }

    [XmlRpcMethod("deleteDevices")]
    private async Task<string> DeleteDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
    {
        _logger.LogDebug("CCU notifies about deleted devices for InterfaceId = {InterfaceId}. Deleted device count: {DeviceDescriptionsCount}",
            interfaceId, deviceDescriptions.Length);
            
        deviceDescriptions
            .ForEach(deviceDescription =>
                _logger.LogTrace("Deleted device: Address = {DeviceDescriptionAddress}", deviceDescription.Address));

        await _eventHandlers.ForEachAsync(x => x.DeleteDevices(deviceDescriptions));
            
        return string.Empty;
    }

    [XmlRpcMethod("updateDevice")]
    private async Task<string> UpdateDevice(string interfaceId, string address, int hint)
    {
        _logger.LogDebug("CCU device is updated. InterfaceId = {InterfaceId}; Address = {Address}; Hint = {Hint}",
            interfaceId, address, hint);

        await _eventHandlers.ForEachAsync(x => x.UpdateDevice(address, hint));
            
        return string.Empty;
    }

    public string ServerUrl { get; set; }
}