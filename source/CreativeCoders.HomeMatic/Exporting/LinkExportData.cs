using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Exporting;

/// <summary>
/// Represents a single communication link as it appears in a device export.
/// </summary>
[PublicAPI]
public class LinkExportData
{
    /// <summary>
    /// Gets the address of the sender of the link.
    /// </summary>
    /// <value>The sender channel or device address.</value>
    public required string Sender { get; init; }

    /// <summary>
    /// Gets the address of the receiver of the link.
    /// </summary>
    /// <value>The receiver channel or device address.</value>
    public required string Receiver { get; init; }

    /// <summary>
    /// Gets the human-readable name of the link.
    /// </summary>
    /// <value>The link name; an empty string if not set on the CCU.</value>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the textual description of the link.
    /// </summary>
    /// <value>The link description; an empty string if not set on the CCU.</value>
    public required string Description { get; init; }
}
