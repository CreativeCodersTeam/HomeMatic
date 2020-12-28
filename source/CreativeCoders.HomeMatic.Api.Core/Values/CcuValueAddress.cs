using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Api.Core.Values
{
    public class CcuValueAddress
    {
        public CcuValueAddress(string deviceAddress, string valueKey)
        {
            Ensure.IsNotNullOrEmpty(deviceAddress, nameof(deviceAddress));
            Ensure.IsNotNullOrEmpty(valueKey, nameof(valueKey));
            
            DeviceAddress = deviceAddress;
            ValueKey = valueKey;
        }
        
        public string DeviceAddress { get; }
        
        public string ValueKey { get; }
    }
}