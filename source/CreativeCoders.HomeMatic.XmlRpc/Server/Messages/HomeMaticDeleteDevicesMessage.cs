using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Messages;

/// <summary>
/// Represents a notification from the HomeMatic CCU that devices have been removed via the <c>deleteDevices</c> XML-RPC callback.
/// </summary>
[PublicAPI]
public class HomeMaticDeleteDevicesMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeMaticDeleteDevicesMessage"/> class.
    /// </summary>
    /// <param name="interfaceId">The identifier of the CCU interface process that sent the notification.</param>
    /// <param name="deviceDescriptions">The descriptions of the deleted devices and channels.</param>
    public HomeMaticDeleteDevicesMessage(string interfaceId, DeviceDescription[] deviceDescriptions)
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
    /// Gets the descriptions of the devices and channels that were removed from the interface process.
    /// </summary>
    /// <value>An array of <see cref="DeviceDescription"/> objects for each deleted device or channel.</value>
    public DeviceDescription[] DeviceDescriptions { get; }
}