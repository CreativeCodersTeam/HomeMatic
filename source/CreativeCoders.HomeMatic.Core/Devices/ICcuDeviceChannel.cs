using System.Collections.Generic;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Devices
{
    [PublicAPI]
    public interface ICcuDeviceChannel : ICcuDeviceBase
    {
        ICcuDevice Parent { get; }
        
        //string ParentType { get; }
        
        int Index { get; }
        
        string Group { get; }
        
        ChannelDirection ChannelDirection { get; }
        
        IEnumerable<string> LinkSourceRoles { get; }
        
        IEnumerable<string> LinkTargetRoles { get; }
    }
}