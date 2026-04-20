using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Devices;

[PublicAPI]
public enum ChannelDirection
{
    None = 0,
    Sender = 1,
    Receiver = 2
}