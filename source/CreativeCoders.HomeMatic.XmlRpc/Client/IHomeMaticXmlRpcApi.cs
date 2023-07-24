using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.Net.XmlRpc.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client
{
    [PublicAPI]
    [GlobalExceptionHandler(typeof(HomeMaticXmlRpcExceptionHandler))]
    public interface IHomeMaticXmlRpcApi
    {
        [XmlRpcMethod("listDevices")]
        Task<IEnumerable<DeviceDescription>> ListDevicesAsync();

        [XmlRpcMethod("getDeviceDescription")]
        Task<DeviceDescription> GetDeviceDescriptionAsync(string address);

        [XmlRpcMethod("getValue")]
        Task<object> GetValueAsync(string address, string valueKey);
        
        [XmlRpcMethod("getValue")]
        Task<T> GetValueAsync<T>(string address, string valueKey);

        [XmlRpcMethod("setValue")]
        Task SetValueAsync(string address, string valueKey, object value);
        
        [XmlRpcMethod("getServiceMessages")]
        Task<IEnumerable<IEnumerable<object>>> GetServiceMessagesAsync();

        [XmlRpcMethod("init")]
        Task InitAsync(string xmlRpcUrl, string interfaceId);

        [XmlRpcMethod("logLevel")]
        Task<int> SetLogLevelAsync(int logLevel);

        [XmlRpcMethod("logLevel")]
        Task<int> GetLogLevelAsync();

        [XmlRpcMethod("getParamset")]
        Task<Dictionary<string, object>> GetParamSetAsync(string address, string paramSetKey);

        [XmlRpcMethod("getParamsetDescription")]
        Task<Dictionary<string, ParameterDescription>> GetParameterDescriptionAsync(string address, string paramSetKey);

        [XmlRpcMethod("listBidcosInterfaces")]
        Task<IEnumerable<BidcosInterface>> ListBidcosInterfacesAsync();

        [XmlRpcMethod("putParamset")]
        Task PutParamsetAsync(string address, string paramSetKey, IDictionary<string, object> paramSet);

        [XmlRpcMethod("ping")]
        Task<bool> PingAsync(string callerId);
        
        [XmlRpcMethod("getVersion")]
        Task<string> GetVersionAsync();
    }
}