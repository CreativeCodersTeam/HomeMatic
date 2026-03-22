using CreativeCoders.Core.Messaging.Messages;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server.Messages;

/// <summary>
/// Represents a value-change or device event received from the HomeMatic CCU via the <c>event</c> XML-RPC callback.
/// </summary>
/// <remarks>
/// The CCU sends this notification whenever a parameter value changes or a device event occurs
/// (e.g. a button press). The <see cref="ValueKey"/> corresponds to an entry in the
/// device's <c>VALUES</c> parameter set description.
/// Special events include <c>UNREACH</c>, <c>STICKY_UNREACH</c>, <c>CONFIG_PENDING</c>, and <c>PONG</c>.
/// </remarks>
[PublicAPI]
public class HomeMaticEventMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeMaticEventMessage"/> class.
    /// </summary>
    /// <param name="interfaceId">The identifier of the CCU interface process that sent the event.</param>
    /// <param name="address">The address of the logical device or channel that generated the event.</param>
    /// <param name="valueKey">The name of the changed value or event key.</param>
    /// <param name="value">The new value or event payload.</param>
    public HomeMaticEventMessage(string interfaceId, string address, string valueKey, object value)
    {
        InterfaceId = interfaceId;
        Address = address;
        ValueKey = valueKey;
        Value = value;
    }

    /// <summary>
    /// Gets the identifier of the CCU interface process that sent the event.
    /// </summary>
    /// <value>The interface identifier as registered via <see cref="Client.IHomeMaticXmlRpcApi.InitAsync"/>.</value>
    public string InterfaceId { get; }

    /// <summary>
    /// Gets the address of the logical device or channel that generated the event.
    /// </summary>
    /// <value>The device or channel address (e.g. <c>ABC1234567:1</c>).</value>
    public string Address { get; }

    /// <summary>
    /// Gets the name of the changed parameter value or event key.
    /// </summary>
    /// <value>A parameter name from the device's <c>VALUES</c> parameter set (e.g. <c>SET_TEMPERATURE</c>).</value>
    public string ValueKey { get; }

    /// <summary>
    /// Gets the new value or event payload.
    /// </summary>
    /// <value>The current value; the data type corresponds to the parameter's type in the <c>VALUES</c> parameter set.</value>
    public object Value { get; }
}