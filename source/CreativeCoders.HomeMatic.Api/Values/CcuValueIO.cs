using System.Threading.Tasks;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Api.Core.Values;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Values
{
    [PublicAPI]
    public class CcuValueIo : ICcuValueIo
    {
        private readonly IHomeMaticXmlRpcApi _homeMaticXmlRpcApi;
        
        public CcuValueIo(IHomeMaticXmlRpcApi homeMaticXmlRpcApi, string deviceAddress, string valueKey) :
            this(homeMaticXmlRpcApi, new CcuValueAddress(deviceAddress, valueKey))
        {
        }

        public CcuValueIo(IHomeMaticXmlRpcApi homeMaticXmlRpcApi, CcuValueAddress valueAddress)
        {
            Ensure.IsNotNull(homeMaticXmlRpcApi, nameof(homeMaticXmlRpcApi));
            Ensure.IsNotNull(valueAddress, nameof(valueAddress));

            _homeMaticXmlRpcApi = homeMaticXmlRpcApi;
            ValueAddress = valueAddress;
        }
        
        public Task WriteAsync(object value)
        {
            return _homeMaticXmlRpcApi.SetValueAsync(ValueAddress.DeviceAddress, ValueAddress.ValueKey, value);
        }

        public Task<object> ReadAsync()
        {
            return _homeMaticXmlRpcApi.GetValueAsync(ValueAddress.DeviceAddress, ValueAddress.ValueKey);
        }

        public Task WriteAsync<T>(T value)
        {
            return _homeMaticXmlRpcApi.SetValueAsync(ValueAddress.DeviceAddress, ValueAddress.ValueKey, value);
        }

        public Task<T> ReadAsync<T>()
        {
            return _homeMaticXmlRpcApi.GetValueAsync<T>(ValueAddress.DeviceAddress, ValueAddress.ValueKey);
        }
        
        public CcuValueAddress ValueAddress { get; set; }
    }

    [PublicAPI]
    public class CcuValueIo<T> : ICcuValueIo<T>, ICcuValueIo
    {
        private readonly IHomeMaticXmlRpcApi _homeMaticXmlRpcApi;

        public CcuValueIo(IHomeMaticXmlRpcApi homeMaticXmlRpcApi, string deviceAddress, string valueKey) :
            this(homeMaticXmlRpcApi, new CcuValueAddress(deviceAddress, valueKey))
        {
        }

        public CcuValueIo(IHomeMaticXmlRpcApi homeMaticXmlRpcApi, CcuValueAddress valueAddress)
        {
            Ensure.IsNotNull(homeMaticXmlRpcApi, nameof(homeMaticXmlRpcApi));
            Ensure.IsNotNull(valueAddress, nameof(valueAddress));
            
            _homeMaticXmlRpcApi = homeMaticXmlRpcApi;
            ValueAddress = valueAddress;
        }
        
        public Task WriteAsync(T value)
        {
            return _homeMaticXmlRpcApi.SetValueAsync(ValueAddress.DeviceAddress, ValueAddress.ValueKey, value);
        }

        Task ICcuValueIo.WriteAsync(object value)
        {
            return _homeMaticXmlRpcApi.SetValueAsync(ValueAddress.DeviceAddress, ValueAddress.ValueKey, value);
        }

        Task<object> ICcuValueIo.ReadAsync()
        {
            return _homeMaticXmlRpcApi.GetValueAsync(ValueAddress.DeviceAddress, ValueAddress.ValueKey);
        }

        Task ICcuValueIo.WriteAsync<TValue>(TValue value)
        {
            return _homeMaticXmlRpcApi.SetValueAsync(ValueAddress.DeviceAddress, ValueAddress.ValueKey, value);
        }

        Task<TValue> ICcuValueIo.ReadAsync<TValue>()
        {
            return _homeMaticXmlRpcApi.GetValueAsync<TValue>(ValueAddress.DeviceAddress, ValueAddress.ValueKey);
        }

        public Task<T> ReadAsync()
        {
            return _homeMaticXmlRpcApi.GetValueAsync<T>(ValueAddress.DeviceAddress, ValueAddress.ValueKey);
        }
        
        public CcuValueAddress ValueAddress { get; set; }
    }
}