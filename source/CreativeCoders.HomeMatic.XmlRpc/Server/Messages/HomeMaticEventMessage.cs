using CreativeCoders.Core.Messaging.Messages;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Messages
{
    [PublicAPI]
    public class HomeMaticEventMessage : MessageBase
    {
        public HomeMaticEventMessage(string interfaceId, string address, string valueKey, object value)
        {
            InterfaceId = interfaceId;
            Address = address;
            ValueKey = valueKey;
            Value = value;
        }
        
        public string InterfaceId { get; }
        
        public string Address { get; }
        
        public string ValueKey { get; }
        
        public object Value { get; }
    }
}