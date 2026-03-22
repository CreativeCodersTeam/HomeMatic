using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Messages;

/// <summary>
/// Represents a notification from the HomeMatic CCU that a device has been updated via the <c>updateDevice</c> XML-RPC callback.
/// </summary>
/// <remarks>
/// The CCU sends this message when a device's configuration or link data has changed.
/// The <see cref="Hint"/> value indicates the type of update:
/// <list type="bullet">
///   <item><description><c>0</c> (UPDATE_HINT_ALL) — all data may have changed.</description></item>
///   <item><description><c>1</c> (UPDATE_HINT_LINKS) — only the link data has changed.</description></item>
/// </list>
/// </remarks>
[PublicAPI]
public class HomeMaticUpdateDeviceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeMaticUpdateDeviceMessage"/> class.
    /// </summary>
    /// <param name="interfaceId">The identifier of the CCU interface process that sent the notification.</param>
    /// <param name="address">The address of the device or channel that was updated.</param>
    /// <param name="hint">A hint that indicates which data changed (0 = all data, 1 = links only).</param>
    public HomeMaticUpdateDeviceMessage(string interfaceId, string address, int hint)
    {
        InterfaceId = interfaceId;
        Address = address;
        Hint = hint;
    }

    /// <summary>
    /// Gets the identifier of the CCU interface process that sent the notification.
    /// </summary>
    /// <value>The interface identifier as registered via <see cref="Client.IHomeMaticXmlRpcApi.InitAsync"/>.</value>
    public string InterfaceId { get; }

    /// <summary>
    /// Gets the address of the device or channel that was updated.
    /// </summary>
    /// <value>The device or channel address (e.g. <c>ABC1234567:1</c>).</value>
    public string Address { get; }

    /// <summary>
    /// Gets the hint value that indicates which data has changed.
    /// </summary>
    /// <value>
    /// <c>0</c> (UPDATE_HINT_ALL) if all device data may have changed;
    /// <c>1</c> (UPDATE_HINT_LINKS) if only link data changed.
    /// </value>
    public int Hint { get; }
}