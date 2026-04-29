using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Links;

/// <summary>
/// Represents the descriptive information of a HomeMatic communication link as returned by
/// the <c>getLinkInfo</c> XML-RPC method.
/// </summary>
/// <remarks>
/// The XML-RPC specification (section 4.3.2) returns this information as a string array of the
/// form <c>[name, description]</c>. This SDK exposes it as a dedicated type for clarity.
/// </remarks>
[PublicAPI]
public class LinkInfo
{
    /// <summary>
    /// Gets or sets the human-readable name of the communication link.
    /// </summary>
    /// <value>The link name; an empty string if not set.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the textual description of the communication link.
    /// </summary>
    /// <value>The link description; an empty string if not set.</value>
    public string Description { get; set; } = string.Empty;
}
