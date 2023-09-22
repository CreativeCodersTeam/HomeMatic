using CreativeCoders.HomeMatic.Api.Core;

namespace CreativeCoders.HomeMatic.Api;

public class ServiceMessage : IServiceMessage
{
    public ServiceMessage(string channelAddress, string messageId, object value)
    {
        ChannelAddress = channelAddress;
        MessageId = messageId;
        Value = value;
    }

    public string ChannelAddress { get; }
        
    public string MessageId { get; }
        
    public object Value { get; }
}