using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Devices
{
    [PublicAPI]
    public enum ChannelDirection
    {
        None = 0,
        Sender = 1,
        Receiver = 2
    }
}