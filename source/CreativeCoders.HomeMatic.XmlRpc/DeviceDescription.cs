using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Definition.MemberConverters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc
{
    [PublicAPI]
    public class DeviceDescription
    {
        [XmlRpcStructMember("ADDRESS", Required = true)]
        public string Address { get; set; }

        [XmlRpcStructMember("TYPE", Required = true)]
        public string DeviceType { get; set; }

        [XmlRpcStructMember("CHILDREN")]
        public string[] Children { get; set; } = new string[0];

        [XmlRpcStructMember("PARENT", Required = true)]
        public string Parent { get; set; }

        [XmlRpcStructMember("PARENT_TYPE", DefaultValue = "")]
        public string ParentType { get; set; } = string.Empty;

        [XmlRpcStructMember("VERSION", Required = true)]
        public int Version { get; set; }

        [XmlRpcStructMember("INDEX")]
        public int Index { get; set; }

        [XmlRpcStructMember("AES_ACTIVE")]
        public bool IsAesActive { get; set; }

        [XmlRpcStructMember("INTERFACE", DefaultValue = "")]
        public string Interface { get; set; } = string.Empty;

        [XmlRpcStructMember("PARAMSETS", DefaultValue = new string[0])]
        public string[] ParamSets { get; set; } = new string[0];

        [XmlRpcStructMember("RX_MODE", DefaultValue = RxMode.None, Converter = typeof(FlagsMemberValueConverter<RxMode>))]
        public RxMode RxMode { get; set; }

        [XmlRpcStructMember("GROUP", DefaultValue = "")]
        public string Group { get; set; } = string.Empty;

        [XmlRpcStructMember("RF_ADDRESS")]
        public int RfAddress { get; set; }

        [XmlRpcStructMember("FIRMWARE", DefaultValue = "")]
        public string Firmware { get; set; } = string.Empty;

        [XmlRpcStructMember("AVAILABLE_FIRMWARE", DefaultValue = "")]
        public string AvailableFirmware { get; set; } = string.Empty;

        [XmlRpcStructMember("UPDATABLE")]
        public bool CanBeUpdated { get; set; }

        [XmlRpcStructMember("FIRMWARE_UPDATE_STATE", DefaultValue = DeviceFirmwareUpdateState.None, Converter = typeof(DeviceFirmwareUpdateStateValueConverter))]
        public DeviceFirmwareUpdateState FirmwareUpdateState { get; set; }

        [XmlRpcStructMember("ROAMING")]
        public bool Roaming { get; set; }

        [XmlRpcStructMember("DIRECTION", DefaultValue = ChannelDirection.None, Converter = typeof(EnumMemberValueConverter<ChannelDirection>))]
        public ChannelDirection ChannelDirection { get; set; }        

        [XmlRpcStructMember("LINK_SOURCE_ROLES", DefaultValue = new string[0], Converter = typeof(LinkRolesValueConverter))]
        public IEnumerable<string> LinkSourceRoles { get; set; }
        
        [XmlRpcStructMember("LINK_TARGET_ROLES", DefaultValue = new string[0], Converter = typeof(LinkRolesValueConverter))]
        public IEnumerable<string> LinkTargetRoles { get; set; }

        public bool IsDevice => string.IsNullOrEmpty(Parent);
        
        public bool IsChannel => !IsDevice;
    }
}