using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Messages;

/// <summary>
/// Represents a notification from the HomeMatic CCU that new devices have been discovered via the <c>newDevices</c> XML-RPC callback.
/// </summary>
/// <remarks>
/// The CCU sends this message when devices are newly registered or when an existing device
/// has changed (e.g. after a firmware update). The logic layer should reconcile its internal
/// state with the provided descriptions.
/// </remarks>
[PublicAPI]
public class HomeMaticNewDevicesMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeMaticNewDevicesMessage"/> class.
    /// </summary>
    /// <param name="interfaceId">The identifier of the CCU interface process that sent the notification.</param>
    /// <param name="deviceDescriptions">The descriptions of the newly discovered or updated devices.</param>
    public HomeMaticNewDevicesMessage(string interfaceId, DeviceDescription[] deviceDescriptions)
    {
        InterfaceId = interfaceId;
        DeviceDescriptions = deviceDescriptions;
    }

    /// <summary>
    /// Gets the identifier of the CCU interface process that sent the notification.
    /// </summary>
    /// <value>The interface identifier as registered via <see cref="Client.IHomeMaticXmlRpcApi.InitAsync"/>.</value>
    public string InterfaceId { get; }

    /// <summary>
    /// Gets the descriptions of the newly discovered or updated devices and channels.
    /// </summary>
    /// <value>An array of <see cref="DeviceDescription"/> objects for each new or changed device.</value>
    public DeviceDescription[] DeviceDescriptions { get; }
}