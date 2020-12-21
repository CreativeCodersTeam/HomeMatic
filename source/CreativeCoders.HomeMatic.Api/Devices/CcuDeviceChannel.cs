using System.Collections.Generic;
using CreativeCoders.HomeMatic.Api.Core.Devices;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Api.Devices
{
    public class CcuDeviceChannel : CcuDeviceBase, ICcuDeviceChannel
    {
        public CcuDeviceChannel(ICcuDevice parent, ICcuDeviceInfo channelInfo, IHomeMaticXmlRpcApi xmlRpcApi) : base(channelInfo, xmlRpcApi)
        {
            Parent = parent;
        }
        
        public ICcuDevice Parent { get; }

        public int Index => DeviceInfo.Index;

        public string Group => DeviceInfo.Group;

        public ChannelDirection ChannelDirection => DeviceInfo.ChannelDirection;

        public IEnumerable<string> LinkSourceRoles => DeviceInfo.LinkSourceRoles;

        public IEnumerable<string> LinkTargetRoles => DeviceInfo.LinkTargetRoles;

        public override bool IsDevice => false;
    }
}