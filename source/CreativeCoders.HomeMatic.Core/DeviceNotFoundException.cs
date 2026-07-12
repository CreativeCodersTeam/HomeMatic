using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc.Exceptions;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Exception that is thrown when a device or channel address cannot be resolved.
/// </summary>
/// <param name="address">The device or channel address that could not be found.</param>
/// <param name="message">An optional error message. If omitted, a default message containing the address is used.</param>
[PublicAPI]
public class DeviceNotFoundException(string address, string? message = null)
    : HomeMaticException(message ?? $"Device with address '{address}' not found.")
{
    /// <summary>
    /// Gets the device or channel address that could not be found.
    /// </summary>
    /// <value>The device or channel address.</value>
    public string Address { get; } = Ensure.NotNull(address);
}
