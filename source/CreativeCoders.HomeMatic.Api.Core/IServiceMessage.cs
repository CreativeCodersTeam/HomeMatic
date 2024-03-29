﻿using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core;

[PublicAPI]
public interface IServiceMessage
{
    string ChannelAddress { get; }
        
    string MessageId { get;}
        
    object Value { get; }
}