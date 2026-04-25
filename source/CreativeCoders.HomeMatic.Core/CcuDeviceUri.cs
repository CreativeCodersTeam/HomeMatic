using System.Diagnostics.CodeAnalysis;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Represents a URI that uniquely identifies a device on a specific CCU.
/// </summary>
[ExcludeFromCodeCoverage]
public class CcuDeviceUri
{
    /// <summary>
    /// Gets the host name or IP address of the CCU.
    /// </summary>
    /// <value>The network host name or IP address of the CCU.</value>
    public required string CcuHost { get; init; }

    /// <summary>
    /// Gets the logical name of the CCU.
    /// </summary>
    /// <value>The human-readable name of the CCU. The default is an empty string.</value>
    public string CcuName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the kind of device addressed by this URI.
    /// </summary>
    /// <value>One of the enumeration values that specifies the device kind.</value>
    public required CcuDeviceKind Kind { get; init; }

    /// <summary>
    /// Gets the device address within the CCU.
    /// </summary>
    /// <value>The device or channel address.</value>
    public required string Address { get; init; }

    /// <summary>
    /// Gets the preferred display name for the CCU host.
    /// </summary>
    /// <value>
    /// The value of <see cref="CcuName"/> if it is not empty; otherwise, the value of <see cref="CcuHost"/>.
    /// </value>
    public string HostDisplayName => string.IsNullOrWhiteSpace(CcuName) ? CcuHost : CcuName;

    /// <summary>
    /// Returns a string representation of this URI in the form <c>{Kind}://{CcuHost}/{Address}</c>.
    /// </summary>
    /// <returns>A string representation of the device URI.</returns>
    public override string ToString()
    {
        return $"{Kind}://{CcuHost}/{Address}";
    }
}
