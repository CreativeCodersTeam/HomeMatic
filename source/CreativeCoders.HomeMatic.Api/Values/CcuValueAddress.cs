using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core.Values;

namespace CreativeCoders.HomeMatic.Api.Values
{
    public class CcuValueAddress : ICcuValueAddress
    {
        public CcuValueAddress(string deviceAddress, string valueKey)
        {
            Ensure.IsNotNullOrWhitespace(deviceAddress, nameof(deviceAddress));
            Ensure.IsNotNullOrWhitespace(valueKey, nameof(valueKey));
            
            DeviceAddress = deviceAddress;
            ValueKey = valueKey;
        }

        public string DeviceAddress { get; }
        
        public string ValueKey { get; }
    }
}