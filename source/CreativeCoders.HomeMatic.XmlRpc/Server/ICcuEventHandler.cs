using System.Threading.Tasks;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

/// <summary>
/// Defines the callbacks that the HomeMatic CCU interface process invokes on a registered logic layer.
/// </summary>
/// <remarks>
/// A logic layer registers itself with the CCU by calling
/// <see cref="Client.IHomeMaticXmlRpcApi.InitAsync"/>. Once registered, the CCU pushes
/// notifications to the logic layer by calling these methods via XML-RPC.
/// Implement this interface and register it with <see cref="ICcuXmlRpcEventServer.RegisterEventHandler"/>
/// to process incoming events.
/// </remarks>
public interface ICcuEventHandler
{
    /// <summary>
    /// Notifies the logic layer that a parameter value has changed or a device event occurred.
    /// </summary>
    /// <param name="address">The address of the logical device or channel that generated the event.</param>
    /// <param name="valueKey">The name of the changed value or event key (e.g. <c>SET_TEMPERATURE</c>, <c>UNREACH</c>).</param>
    /// <param name="value">The new value or the event payload; the type corresponds to the parameter's data type.</param>
    Task Event(string address, string valueKey, object value);

    /// <summary>
    /// Notifies the logic layer that new devices have been discovered by the interface process.
    /// </summary>
    /// <param name="deviceDescriptions">
    /// An array of <see cref="DeviceDescription"/> objects for the newly found devices and channels.
    /// If a description refers to a device already known to the logic layer, the device may have changed
    /// (e.g. after a firmware update) and the logic layer should reconcile its state.
    /// </param>
    Task NewDevices(DeviceDescription[] deviceDescriptions);

    /// <summary>
    /// Notifies the logic layer that devices have been removed from the interface process.
    /// </summary>
    /// <param name="deviceDescriptions">
    /// An array of <see cref="DeviceDescription"/> objects for the deleted devices and channels.
    /// </param>
    Task DeleteDevices(DeviceDescription[] deviceDescriptions);

    /// <summary>
    /// Notifies the logic layer that a device or channel has been updated.
    /// </summary>
    /// <param name="address">The address of the device or channel that was updated.</param>
    /// <param name="hint">
    /// A value that specifies the kind of change:
    /// <c>0</c> (UPDATE_HINT_ALL) means an unspecified change occurred;
    /// <c>1</c> (UPDATE_HINT_LINKS) means the number of link partners changed.
    /// </param>
    Task UpdateDevice(string address, int hint);
}