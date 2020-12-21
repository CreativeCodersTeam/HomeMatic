using CreativeCoders.Net.XmlRpc.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc
{
    [PublicAPI]
    public class BidcosInterface
    {
        [XmlRpcStructMember("ADDRESS", DefaultValue = "")]
        public string Address { get; set; } = string.Empty;

        [XmlRpcStructMember("DESCRIPTION", DefaultValue = "")]
        public string Description { get; set; } = string.Empty;

        [XmlRpcStructMember("CONNECTED")]
        public bool IsConnected { get; set; }

        [XmlRpcStructMember("DEFAULT")]
        public bool IsDefault { get; set; }
    }
}