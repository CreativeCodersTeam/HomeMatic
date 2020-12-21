using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.Core;
using CreativeCoders.Core.Logging;
using CreativeCoders.Core.Threading;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Server;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server
{
    [PublicAPI]
    public class CcuXmlRpcEventServer : ICcuXmlRpcEventServer
    {
        private static readonly ILogger Log = LogManager.GetLogger<CcuXmlRpcEventServer>();
        
        private readonly IXmlRpcServer _xmlRpcServer;
        
        private IList<ICcuEventHandler> _eventHandlers;

        public CcuXmlRpcEventServer(IXmlRpcServer xmlRpcServer)
        {
            _xmlRpcServer = xmlRpcServer;

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
            Log.Debug(
                $"Event from CCU received. InterfaceId = {interfaceId}; Address = {address}; ValueKey = {valueKey}; Value = {value}");

            await _eventHandlers.ForEachAsync(x => x.Event(address, valueKey, value));

            return string.Empty;
        }

        [XmlRpcMethod("listDevices")]
        private Task<IEnumerable<XmlRpcValue>> ListDevices(string interfaceId)
        {
            Log.Debug($"List devices for InterfaceId = {interfaceId}");
            
            return Task.FromResult(new XmlRpcValue[0] as IEnumerable<XmlRpcValue>);
        }

        [XmlRpcMethod("newDevices")]
        private async Task<string> NewDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
        {
            Log.Debug(
                $"CCU notifies about new Devices for InterfaceId = {interfaceId}. New device count: {deviceDescriptions.Length}");
            
            deviceDescriptions.ForEach(deviceDescription => Log.Trace($"New device: Address = {deviceDescription.Address}"));

            await _eventHandlers.ForEachAsync(x => x.NewDevices(deviceDescriptions));
            
            return string.Empty;
        }

        [XmlRpcMethod("deleteDevices")]
        private async Task<string> DeleteDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
        {
            Log.Debug($"CCU notifies about deleted devices for InterfaceId = {interfaceId}. Deleted device count: {deviceDescriptions.Length}");
            
            deviceDescriptions.ForEach(deviceDescription => Log.Trace($"Deleted device: Address = {deviceDescription.Address}"));

            await _eventHandlers.ForEachAsync(x => x.DeleteDevices(deviceDescriptions));
            
            return string.Empty;
        }

        [XmlRpcMethod("updateDevice")]
        private async Task<string> UpdateDevice(string interfaceId, string address, int hint)
        {
            Log.Debug($"CCU device is updated. InterfaceId = {interfaceId}; Address = {address}; Hint = {hint}");

            await _eventHandlers.ForEachAsync(x => x.UpdateDevice(address, hint));
            
            return string.Empty;
        }

        public string ServerUrl { get; set; }
    }
}