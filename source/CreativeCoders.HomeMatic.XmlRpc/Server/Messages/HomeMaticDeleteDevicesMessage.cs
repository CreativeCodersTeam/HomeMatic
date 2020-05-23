﻿using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Messages
{
    [PublicAPI]
    public class HomeMaticDeleteDevicesMessage
    {
        public HomeMaticDeleteDevicesMessage(string interfaceId, DeviceDescription[] deviceDescriptions)
        {
            InterfaceId = interfaceId;
            DeviceDescriptions = deviceDescriptions;
        }
        
        public string InterfaceId { get; }
        
        public DeviceDescription[] DeviceDescriptions { get; }
    }
}