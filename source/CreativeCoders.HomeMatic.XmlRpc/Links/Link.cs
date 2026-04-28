using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.Net.XmlRpc.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Links;

/// <summary>
/// Describes a HomeMatic communication link between two logical devices or channels as returned
/// by the <c>getLinks</c> XML-RPC method.
/// </summary>
/// <remarks>
/// See section 4.2.10 of the HomeMatic XML-RPC specification. The <see cref="SenderParamSet"/>
/// and <see cref="ReceiverParamSet"/> fields are only populated when the corresponding
/// <see cref="GetLinksFlags.SenderParamSet"/> or <see cref="GetLinksFlags.ReceiverParamSet"/> flag
/// is passed to <c>getLinks</c>; otherwise they default to an empty dictionary.
/// </remarks>
[PublicAPI]
public class Link
{
    /// <summary>
    /// Gets or sets the address of the sender of this communication link.
    /// </summary>
    /// <value>The channel or device address of the sender (e.g. <c>ABC1234567:1</c>).</value>
    [XmlRpcStructMember("SENDER", Required = true)]
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the receiver of this communication link.
    /// </summary>
    /// <value>The channel or device address of the receiver (e.g. <c>ABC1234567:2</c>).</value>
    [XmlRpcStructMember("RECEIVER", Required = true)]
    public string Receiver { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state flags of this communication link.
    /// </summary>
    /// <value>A bitwise combination of <see cref="LinkFlags"/> values.</value>
    [XmlRpcStructMember("FLAGS", DefaultValue = LinkFlags.None,
        Converter = typeof(FlagsMemberValueConverter<LinkFlags>))]
    public LinkFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the human-readable name of this communication link.
    /// </summary>
    /// <value>The link name; an empty string if not set.</value>
    [XmlRpcStructMember("NAME", DefaultValue = "")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the textual description of this communication link.
    /// </summary>
    /// <value>The link description; an empty string if not set.</value>
    [XmlRpcStructMember("DESCRIPTION", DefaultValue = "")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter set associated with the sender side of this communication link.
    /// </summary>
    /// <value>
    /// A dictionary mapping parameter names to their current values. Only populated when the
    /// <see cref="GetLinksFlags.SenderParamSet"/> flag was passed to <c>getLinks</c>; otherwise
    /// an empty dictionary.
    /// </value>
    [XmlRpcStructMember("SENDER_PARAMSET")]
    public Dictionary<string, object> SenderParamSet { get; set; } = new();

    /// <summary>
    /// Gets or sets the parameter set associated with the receiver side of this communication link.
    /// </summary>
    /// <value>
    /// A dictionary mapping parameter names to their current values. Only populated when the
    /// <see cref="GetLinksFlags.ReceiverParamSet"/> flag was passed to <c>getLinks</c>; otherwise
    /// an empty dictionary.
    /// </value>
    [XmlRpcStructMember("RECEIVER_PARAMSET")]
    public Dictionary<string, object> ReceiverParamSet { get; set; } = new();
}
