using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using CreativeCoders.Net.XmlRpc.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

/// <summary>
/// Defines the HomeMatic XML-RPC API for communicating with a CCU interface process.
/// </summary>
/// <remarks>
/// This interface maps to the XML-RPC methods exposed by the HomeMatic CCU on port 2001 (BidCoS-RF)
/// or port 2000 (BidCoS-Wired). Use <see cref="IHomeMaticXmlRpcApiBuilder"/> to create an instance.
/// XML-RPC fault responses are automatically translated to typed exceptions by
/// <see cref="HomeMaticXmlRpcExceptionHandler"/>.
/// </remarks>
[PublicAPI]
[GlobalExceptionHandler(typeof(HomeMaticXmlRpcExceptionHandler))]
public interface IHomeMaticXmlRpcApi
{
    /// <summary>
    /// Retrieves descriptions of all logical devices known to the interface process.
    /// </summary>
    /// <returns>A collection of <see cref="DeviceDescription"/> objects for all known devices and channels.</returns>
    [XmlRpcMethod("listDevices")]
    Task<IEnumerable<DeviceDescription>> ListDevicesAsync();

    /// <summary>
    /// Retrieves the description of a single logical device or channel.
    /// </summary>
    /// <param name="address">The address of the device or channel to retrieve (e.g. <c>ABC1234567</c> or <c>ABC1234567:1</c>).</param>
    /// <returns>The <see cref="DeviceDescription"/> for the specified address.</returns>
    [XmlRpcMethod("getDeviceDescription")]
    Task<DeviceDescription> GetDeviceDescriptionAsync(string address);

    /// <summary>
    /// Reads a single value from the <c>VALUES</c> parameter set of a logical device.
    /// </summary>
    /// <param name="address">The address of the logical device or channel.</param>
    /// <param name="valueKey">The name of the value to read (e.g. <c>SET_TEMPERATURE</c>).</param>
    /// <returns>The current value as an untyped object.</returns>
    [XmlRpcMethod("getValue")]
    Task<object> GetValueAsync(string address, string valueKey);

    /// <summary>
    /// Reads a single value from the <c>VALUES</c> parameter set of a logical device as a specific type.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="address">The address of the logical device or channel.</param>
    /// <param name="valueKey">The name of the value to read (e.g. <c>SET_TEMPERATURE</c>).</param>
    /// <returns>The current value cast to <typeparamref name="T"/>.</returns>
    [XmlRpcMethod("getValue")]
    Task<T> GetValueAsync<T>(string address, string valueKey);

    /// <summary>
    /// Writes a single value to the <c>VALUES</c> parameter set of a logical device.
    /// </summary>
    /// <param name="address">The address of the logical device or channel.</param>
    /// <param name="valueKey">The name of the value to write (e.g. <c>SET_TEMPERATURE</c>).</param>
    /// <param name="value">The value to write.</param>
    [XmlRpcMethod("setValue")]
    Task SetValueAsync(string address, string valueKey, object value);

    /// <summary>
    /// Retrieves all current service messages from the interface process.
    /// </summary>
    /// <returns>
    /// A collection of service message arrays. Each inner array contains three elements:
    /// the channel address (string), the service message ID (string, e.g. <c>UNREACH</c>),
    /// and the current value.
    /// </returns>
    [XmlRpcMethod("getServiceMessages")]
    Task<IEnumerable<IEnumerable<object>>> GetServiceMessagesAsync();

    /// <summary>
    /// Registers this logic layer with the interface process to receive event callbacks.
    /// </summary>
    /// <param name="xmlRpcUrl">The URL of the XML-RPC server hosted by the logic layer that will receive callbacks.</param>
    /// <param name="interfaceId">
    /// The identifier the interface process uses when calling back into the logic layer.
    /// Pass an empty string to unregister from event notifications.
    /// </param>
    /// <remarks>
    /// After calling this method the interface process will push device changes, new devices,
    /// and deleted devices to the logic layer via the callbacks defined by <see cref="Server.ICcuEventHandler"/>.
    /// </remarks>
    [XmlRpcMethod("init")]
    Task InitAsync(string xmlRpcUrl, string interfaceId);

    /// <summary>
    /// Sets the log level of the interface process.
    /// </summary>
    /// <param name="logLevel">
    /// The new log level: 0 = all, 1 = debug, 2 = info, 3 = notice, 4 = warning, 5 = error, 6 = fatal error.
    /// </param>
    /// <returns>The log level that was active before the change.</returns>
    [XmlRpcMethod("logLevel")]
    Task<int> SetLogLevelAsync(int logLevel);

    /// <summary>
    /// Retrieves the current log level of the interface process.
    /// </summary>
    /// <returns>The active log level value.</returns>
    [XmlRpcMethod("logLevel")]
    Task<int> GetLogLevelAsync();

    /// <summary>
    /// Reads all values of a parameter set for a logical device.
    /// </summary>
    /// <param name="address">The address of the logical device or channel.</param>
    /// <param name="paramSetKey">
    /// The parameter set to read: <c>MASTER</c> for configuration, <c>VALUES</c> for dynamic values,
    /// or the address of a link peer for a LINK parameter set.
    /// </param>
    /// <returns>A dictionary mapping parameter names to their current values.</returns>
    [XmlRpcMethod("getParamset")]
    Task<Dictionary<string, object>> GetParamSetAsync(string address, string paramSetKey);

    /// <summary>
    /// Retrieves the description of all parameters in a parameter set.
    /// </summary>
    /// <param name="address">The address of the logical device or channel.</param>
    /// <param name="paramSetKey">
    /// The parameter set to describe: <c>MASTER</c>, <c>VALUES</c>, or a link peer address.
    /// </param>
    /// <returns>
    /// A dictionary mapping each parameter name to its <see cref="ParameterDescription"/>.
    /// </returns>
    [XmlRpcMethod("getParamsetDescription")]
    Task<Dictionary<string, ParameterDescription>> GetParameterDescriptionAsync(string address, string paramSetKey);

    /// <summary>
    /// Retrieves all BidCoS interfaces connected to the CCU.
    /// </summary>
    /// <returns>A collection of <see cref="BidcosInterface"/> objects describing each available interface.</returns>
    [XmlRpcMethod("listBidcosInterfaces")]
    Task<IEnumerable<BidcosInterface>> ListBidcosInterfacesAsync();

    /// <summary>
    /// Writes an entire parameter set for a logical device.
    /// </summary>
    /// <param name="address">The address of the logical device or channel.</param>
    /// <param name="paramSetKey">
    /// The parameter set to write: <c>MASTER</c> for configuration, <c>VALUES</c> for dynamic values,
    /// or the address of a link peer for a LINK parameter set.
    /// </param>
    /// <param name="paramSet">
    /// A dictionary of parameter names and values to write. Parameters not present in the dictionary
    /// retain their existing values.
    /// </param>
    [XmlRpcMethod("putParamset")]
    Task PutParamsetAsync(string address, string paramSetKey, IDictionary<string, object> paramSet);

    /// <summary>
    /// Sends a ping to the interface process and triggers a PONG event to all registered logic layers.
    /// </summary>
    /// <param name="callerId">An arbitrary string that is returned as the value of the PONG event.</param>
    /// <returns>
    /// <see langword="true"/> if no exception occurred during processing; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The PONG event is delivered via the <c>event</c> callback with address <c>CENTRAL</c> and key <c>PONG</c>.
    /// </remarks>
    [XmlRpcMethod("ping")]
    Task<bool> PingAsync(string callerId);

    /// <summary>
    /// Retrieves the version string of the BidCoS service running on the CCU.
    /// </summary>
    /// <returns>The version string of the interface process.</returns>
    [XmlRpcMethod("getVersion")]
    Task<string> GetVersionAsync();

    /// <summary>
    /// Retrieves all communication links assigned to a logical device or channel.
    /// </summary>
    /// <param name="address">
    /// The channel or device address. Pass an empty string to retrieve all links of the entire
    /// interface process.
    /// </param>
    /// <param name="flags">
    /// A bitwise combination of <see cref="GetLinksFlags"/> values cast to <see cref="int"/>. See
    /// <see cref="HomeMaticXmlRpcApiLinkExtensions.GetLinksAsync(IHomeMaticXmlRpcApi, string, GetLinksFlags)"/>
    /// for a strongly-typed overload.
    /// </param>
    /// <returns>A collection of <see cref="Link"/> structures describing each link.</returns>
    /// <remarks>See section 4.2.10 of the HomeMatic XML-RPC specification.</remarks>
    [XmlRpcMethod("getLinks")]
    Task<IEnumerable<Link>> GetLinksAsync(string address, int flags);

    /// <summary>
    /// Creates a communication link between two logical devices or channels.
    /// </summary>
    /// <param name="sender">The address of the sender of the link.</param>
    /// <param name="receiver">The address of the receiver of the link.</param>
    /// <param name="name">An optional name for the link. Pass an empty string when not used.</param>
    /// <param name="description">An optional description for the link. Pass an empty string when not used.</param>
    /// <remarks>See section 4.2.11 of the HomeMatic XML-RPC specification.</remarks>
    [XmlRpcMethod("addLink")]
    Task AddLinkAsync(string sender, string receiver, string name, string description);

    /// <summary>
    /// Removes the communication link between two logical devices or channels.
    /// </summary>
    /// <param name="sender">The address of the sender of the link.</param>
    /// <param name="receiver">The address of the receiver of the link.</param>
    /// <remarks>See section 4.2.12 of the HomeMatic XML-RPC specification.</remarks>
    [XmlRpcMethod("removeLink")]
    Task RemoveLinkAsync(string sender, string receiver);

    /// <summary>
    /// Updates the descriptive texts of an existing communication link.
    /// </summary>
    /// <param name="sender">The address of the sender of the link.</param>
    /// <param name="receiver">The address of the receiver of the link.</param>
    /// <param name="name">The new name of the link.</param>
    /// <param name="description">The new description of the link.</param>
    /// <remarks>
    /// See section 4.3.1 of the HomeMatic XML-RPC specification. This method is supported by
    /// BidCoS-RF and BidCoS-Wired interface processes.
    /// </remarks>
    [XmlRpcMethod("setLinkInfo")]
    Task SetLinkInfoAsync(string sender, string receiver, string name, string description);

    /// <summary>
    /// Retrieves the raw <c>[name, description]</c> tuple of an existing communication link as
    /// returned by the XML-RPC interface.
    /// </summary>
    /// <param name="senderAddress">The address of the sender of the link.</param>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <returns>A two-element string sequence: the link name followed by the link description.</returns>
    /// <remarks>
    /// See section 4.3.2 of the HomeMatic XML-RPC specification. Prefer the strongly-typed
    /// extension method
    /// <see cref="HomeMaticXmlRpcApiLinkExtensions.GetLinkInfoAsync(IHomeMaticXmlRpcApi, string, string)"/>
    /// which returns a <see cref="LinkInfo"/> instance.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [XmlRpcMethod("getLinkInfo")]
    Task<IEnumerable<string>> GetLinkInfoRawAsync(string senderAddress, string receiverAddress);

    /// <summary>
    /// Activates a link parameter set so that the logical device behaves as if it had been
    /// triggered directly by the assigned communication partner.
    /// </summary>
    /// <param name="address">The address of the logical device whose link parameter set should be activated.</param>
    /// <param name="peerAddress">The address of the communication partner whose link parameter set is activated.</param>
    /// <param name="longPress">
    /// <see langword="true"/> to activate the parameter set for a long key press; otherwise <see langword="false"/>.
    /// </param>
    /// <remarks>
    /// See section 4.3.3 of the HomeMatic XML-RPC specification. This method is supported by
    /// BidCoS-RF interface processes only.
    /// </remarks>
    [XmlRpcMethod("activateLinkParamset")]
    Task ActivateLinkParamsetAsync(string address, string peerAddress, bool longPress);

    /// <summary>
    /// Retrieves all communication partners assigned to a logical device.
    /// </summary>
    /// <param name="address">The address of the logical device.</param>
    /// <returns>
    /// A collection of peer addresses. Each entry can be used as the <c>paramSetKey</c> argument
    /// of <see cref="GetParamSetAsync"/> and <see cref="PutParamsetAsync"/>.
    /// </returns>
    /// <remarks>See section 4.3.20 of the HomeMatic XML-RPC specification.</remarks>
    [XmlRpcMethod("getLinkPeers")]
    Task<IEnumerable<string>> GetLinkPeersAsync(string address);
}