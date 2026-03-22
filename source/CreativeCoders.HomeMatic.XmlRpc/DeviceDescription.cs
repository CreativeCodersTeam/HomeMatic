using System;
using System.Collections.Generic;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Definition.MemberConverters;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc;

/// <summary>
/// Represents the description of a HomeMatic logical device or channel as returned by the XML-RPC interface.
/// </summary>
/// <remarks>
/// In HomeMatic, every physical device and each of its channels are treated as individual logical
/// devices with unique addresses. For example, a multi-function device with address <c>ABC1234567</c>
/// exposes one entry for the device itself and one entry per channel (e.g. <c>ABC1234567:0</c>,
/// <c>ABC1234567:1</c>, etc.).
/// <para>
/// Use <see cref="IsDevice"/> and <see cref="IsChannel"/> to distinguish between a top-level device
/// and a channel entry.
/// </para>
/// </remarks>
[PublicAPI]
public class DeviceDescription
{
    /// <summary>
    /// Gets or sets the unique address of the device or channel.
    /// </summary>
    /// <value>The address string (e.g. <c>ABC1234567</c> for a device or <c>ABC1234567:1</c> for a channel).</value>
    [XmlRpcStructMember("ADDRESS", Required = true)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type identifier of the device or channel.
    /// </summary>
    /// <value>The short type name (e.g. <c>HM-CC-RT-DN</c>).</value>
    [XmlRpcStructMember("TYPE", Required = true)]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the addresses of all subordinate channels belonging to this device.
    /// </summary>
    /// <value>An array of channel addresses. Empty for channel entries.</value>
    [XmlRpcStructMember("CHILDREN")]
    public string[] Children { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the address of the parent device.
    /// </summary>
    /// <value>
    /// The address of the parent device for a channel; an empty string for a top-level device.
    /// </value>
    [XmlRpcStructMember("PARENT", Required = true)]
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type identifier of the parent device. Only present for channels.
    /// </summary>
    /// <value>The short type name of the parent device.</value>
    [XmlRpcStructMember("PARENT_TYPE", DefaultValue = "")]
    public string ParentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version number of the device or channel description.
    /// </summary>
    /// <value>The version of the device or channel description as reported by the interface process.</value>
    [XmlRpcStructMember("VERSION", Required = true)]
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the channel index within the parent device. Only present for channels.
    /// </summary>
    /// <value>The zero-based channel number.</value>
    [XmlRpcStructMember("INDEX")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether AES-secured transmission is enabled for this channel.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if AES encryption is active for the channel; otherwise, <see langword="false"/>.
    /// </value>
    [XmlRpcStructMember("AES_ACTIVE")]
    public bool IsAesActive { get; set; }

    /// <summary>
    /// Gets or sets the serial number of the BidCoS interface assigned to this device. Only present for BidCoS-RF devices.
    /// </summary>
    /// <value>The serial number of the assigned BidCoS-RF interface.</value>
    [XmlRpcStructMember("INTERFACE", DefaultValue = "")]
    public string Interface { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the names of the parameter sets available for this device or channel.
    /// </summary>
    /// <value>
    /// An array of parameter set keys, typically containing <c>MASTER</c>, <c>VALUES</c>, and/or <c>LINK</c>.
    /// </value>
    [XmlRpcStructMember("PARAMSETS", DefaultValue = new string[0])]
    public string[] ParamSets { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the reception mode flags for this device. Only present for BidCoS-RF devices.
    /// </summary>
    /// <value>A bitwise combination of <see cref="CreativeCoders.HomeMatic.Core.Parameters.RxMode"/> values.</value>
    [XmlRpcStructMember("RX_MODE", DefaultValue = RxMode.None, Converter = typeof(FlagsMemberValueConverter<RxMode>))]
    public RxMode RxMode { get; set; }

    /// <summary>
    /// Gets or sets the address of the paired channel in a button group. Only present for grouped channels.
    /// </summary>
    /// <value>The address of the other channel belonging to the same button group.</value>
    [XmlRpcStructMember("GROUP", DefaultValue = "")]
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the RF address of the device. Only present for devices.
    /// </summary>
    /// <value>The numeric RF address used on the BidCoS radio bus.</value>
    [XmlRpcStructMember("RF_ADDRESS")]
    public int RfAddress { get; set; }

    /// <summary>
    /// Gets or sets the currently installed firmware version. Only present for devices.
    /// </summary>
    /// <value>The firmware version string (e.g. <c>1.4</c>).</value>
    [XmlRpcStructMember("FIRMWARE", DefaultValue = "")]
    public string Firmware { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the firmware version available for update. Only present for devices.
    /// </summary>
    /// <value>The available firmware version string, or empty if no update is available.</value>
    [XmlRpcStructMember("AVAILABLE_FIRMWARE", DefaultValue = "")]
    public string AvailableFirmware { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value that indicates whether the device firmware can be updated. Only present for devices.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the device supports firmware updates; otherwise, <see langword="false"/>.
    /// </value>
    [XmlRpcStructMember("UPDATABLE")]
    public bool CanBeUpdated { get; set; }

    /// <summary>
    /// Gets or sets the current firmware update state of the device. Only present for devices.
    /// </summary>
    /// <value>One of the <see cref="DeviceFirmwareUpdateState"/> values indicating the update progress.</value>
    [XmlRpcStructMember("FIRMWARE_UPDATE_STATE", DefaultValue = DeviceFirmwareUpdateState.None, Converter = typeof(DeviceFirmwareUpdateStateValueConverter))]
    public DeviceFirmwareUpdateState FirmwareUpdateState { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the interface assignment is adjusted automatically based on signal strength.
    /// Only present for BidCoS-RF devices.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the device uses automatic interface roaming; otherwise, <see langword="false"/>.
    /// </value>
    [XmlRpcStructMember("ROAMING")]
    public bool Roaming { get; set; }

    /// <summary>
    /// Gets or sets the direction of this channel in a direct device link. Only present for channels.
    /// </summary>
    /// <value>One of the <see cref="CreativeCoders.HomeMatic.Core.Devices.ChannelDirection"/> values.</value>
    [XmlRpcStructMember("DIRECTION", DefaultValue = ChannelDirection.None, Converter = typeof(EnumMemberValueConverter<ChannelDirection>))]
    public ChannelDirection ChannelDirection { get; set; }

    /// <summary>
    /// Gets or sets the roles this channel can assume as a sender in a direct device link. Only present for channels.
    /// </summary>
    /// <value>A collection of role names (e.g. <c>SWITCH</c>) separated by spaces in the raw XML-RPC data.</value>
    [XmlRpcStructMember("LINK_SOURCE_ROLES", DefaultValue = new string[0],
        Converter = typeof(LinkRolesValueConverter))]
    public IEnumerable<string> LinkSourceRoles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the roles this channel can assume as a receiver in a direct device link. Only present for channels.
    /// </summary>
    /// <value>A collection of role names (e.g. <c>SWITCH</c>) separated by spaces in the raw XML-RPC data.</value>
    [XmlRpcStructMember("LINK_TARGET_ROLES", DefaultValue = new string[0], Converter = typeof(LinkRolesValueConverter))]
    public IEnumerable<string> LinkTargetRoles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets a value that indicates whether this description represents a top-level device (not a channel).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see cref="Parent"/> is empty; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsDevice => string.IsNullOrEmpty(Parent);

    /// <summary>
    /// Gets a value that indicates whether this description represents a channel of a device.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this entry describes a channel; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsChannel => !IsDevice;
}