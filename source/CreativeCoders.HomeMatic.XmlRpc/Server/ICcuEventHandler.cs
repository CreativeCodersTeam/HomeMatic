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
/// <para>
/// Every callback receives the interface identifier that the logic layer passed to
/// <see cref="Client.IHomeMaticXmlRpcApi.InitAsync"/>, so a single handler can tell several registered
/// interfaces apart.
/// </para>
/// <para>
/// Implementations must not throw. The CCU sends events in <c>system.multicall</c> batches, and a single
/// throwing callback fails the whole batch with HTTP 500.
/// </para>
/// </remarks>
public interface ICcuEventHandler
{
    /// <summary>
    /// Notifies the logic layer that a parameter value has changed or a device event occurred.
    /// </summary>
    /// <param name="interfaceId">The interface identifier that the logic layer passed to <c>init</c>.</param>
    /// <param name="address">The address of the logical device or channel that generated the event.</param>
    /// <param name="valueKey">The name of the changed value or event key (e.g. <c>SET_TEMPERATURE</c>, <c>UNREACH</c>).</param>
    /// <param name="value">The new value or the event payload; the type corresponds to the parameter's data type.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Event(string interfaceId, string address, string valueKey, object value);

    /// <summary>
    /// Notifies the logic layer that new devices have been discovered by the interface process.
    /// </summary>
    /// <param name="interfaceId">The interface identifier that the logic layer passed to <c>init</c>.</param>
    /// <param name="deviceDescriptions">
    /// An array of <see cref="DeviceDescription"/> objects for the newly found devices and channels.
    /// If a description refers to a device already known to the logic layer, the device may have changed
    /// (e.g. after a firmware update) and the logic layer should reconcile its state.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NewDevices(string interfaceId, DeviceDescription[] deviceDescriptions);

    /// <summary>
    /// Notifies the logic layer that devices have been removed from the interface process.
    /// </summary>
    /// <param name="interfaceId">The interface identifier that the logic layer passed to <c>init</c>.</param>
    /// <param name="deviceDescriptions">
    /// An array of <see cref="DeviceDescription"/> objects for the deleted devices and channels.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteDevices(string interfaceId, DeviceDescription[] deviceDescriptions);

    /// <summary>
    /// Notifies the logic layer that a device or channel has been updated.
    /// </summary>
    /// <param name="interfaceId">The interface identifier that the logic layer passed to <c>init</c>.</param>
    /// <param name="address">The address of the device or channel that was updated.</param>
    /// <param name="hint">
    /// A value that specifies the kind of change:
    /// <c>0</c> (UPDATE_HINT_ALL) means an unspecified change occurred;
    /// <c>1</c> (UPDATE_HINT_LINKS) means the number of link partners changed.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateDevice(string interfaceId, string address, int hint);
}