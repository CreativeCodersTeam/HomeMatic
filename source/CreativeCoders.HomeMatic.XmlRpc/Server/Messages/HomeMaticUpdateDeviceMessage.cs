using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Messages
{
    [PublicAPI]
    public class HomeMaticUpdateDeviceMessage
    {
        public HomeMaticUpdateDeviceMessage(string interfaceId, string address, int hint)
        {
            InterfaceId = interfaceId;
            Address = address;
            Hint = hint;
        }
        
        public string InterfaceId { get; }
        
        public string Address { get; }
        
        public int Hint { get; }
    }
}