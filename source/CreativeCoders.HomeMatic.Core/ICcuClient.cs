using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Links;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Provides access to the devices of a single HomeMatic CCU.
/// </summary>
public interface ICcuClient
{
    /// <summary>
    /// Asynchronously retrieves all devices known to the CCU.
    /// </summary>
    /// <returns>A task that yields an enumerable of <see cref="ICcuDevice"/> instances.</returns>
    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    /// <summary>
    /// Asynchronously retrieves a single device by its address.
    /// </summary>
    /// <param name="address">The device address.</param>
    /// <returns>A task that yields the matching <see cref="ICcuDevice"/>.</returns>
    /// <exception cref="DeviceNotFoundException">Thrown when the CCU does not know a device with the given address.</exception>
    Task<ICcuDevice> GetDeviceAsync(string address);

    /// <summary>
    /// Asynchronously retrieves all devices including their parameter descriptions.
    /// </summary>
    /// <param name="buildOptions">Optional build options controlling whether links are fetched.</param>
    /// <returns>A task that yields an enumerable of <see cref="ICompleteCcuDevice"/> instances.</returns>
    Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync(
        CompleteCcuDeviceBuildOptions? buildOptions = null);

    /// <summary>
    /// Asynchronously retrieves a single device including its parameter descriptions.
    /// </summary>
    /// <param name="address">The device address.</param>
    /// <param name="buildOptions">Optional build options controlling whether links are fetched.</param>
    /// <returns>A task that yields the matching <see cref="ICompleteCcuDevice"/>.</returns>
    /// <exception cref="DeviceNotFoundException">Thrown when the CCU does not know a device with the given address.</exception>
    Task<ICompleteCcuDevice> GetCompleteDeviceAsync(string address,
        CompleteCcuDeviceBuildOptions? buildOptions = null);

    /// <summary>
    /// Asynchronously retrieves all communication links known to the CCU interface process of the
    /// specified device kind.
    /// </summary>
    /// <param name="kind">The device kind whose interface process is queried.</param>
    /// <param name="flags">A bitwise combination of <see cref="GetLinksFlags"/> values controlling the level of detail.</param>
    /// <returns>A task that yields a collection of <see cref="Link"/> structures describing each link.</returns>
    Task<IEnumerable<Link>> GetAllLinksAsync(CcuDeviceKind kind = CcuDeviceKind.HomeMatic,
        GetLinksFlags flags = GetLinksFlags.None);

    /// <summary>
    /// Asynchronously creates a communication link between two logical channels or devices.
    /// </summary>
    /// <param name="senderAddress">The address of the sender of the link.</param>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <param name="name">An optional name for the link.</param>
    /// <param name="description">An optional description for the link.</param>
    /// <param name="kind">The device kind whose interface process performs the operation.</param>
    /// <returns>A task that completes when the link has been created.</returns>
    Task AddLinkAsync(string senderAddress, string receiverAddress, string name = "",
        string description = "", CcuDeviceKind kind = CcuDeviceKind.HomeMatic);

    /// <summary>
    /// Asynchronously removes the communication link between two logical channels or devices.
    /// </summary>
    /// <param name="senderAddress">The address of the sender of the link.</param>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <param name="kind">The device kind whose interface process performs the operation.</param>
    /// <returns>A task that completes when the link has been removed.</returns>
    Task RemoveLinkAsync(string senderAddress, string receiverAddress,
        CcuDeviceKind kind = CcuDeviceKind.HomeMatic);

    /// <summary>
    /// Asynchronously updates the descriptive texts of an existing communication link.
    /// </summary>
    /// <param name="senderAddress">The address of the sender of the link.</param>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <param name="name">The new name of the link.</param>
    /// <param name="description">The new description of the link.</param>
    /// <param name="kind">The device kind whose interface process performs the operation.</param>
    /// <returns>A task that completes when the link has been updated.</returns>
    Task SetLinkInfoAsync(string senderAddress, string receiverAddress, string name,
        string description, CcuDeviceKind kind = CcuDeviceKind.HomeMatic);

    /// <summary>
    /// Asynchronously retrieves the descriptive information of an existing communication link.
    /// </summary>
    /// <param name="senderAddress">The address of the sender of the link.</param>
    /// <param name="receiverAddress">The address of the receiver of the link.</param>
    /// <param name="kind">The device kind whose interface process performs the operation.</param>
    /// <returns>A task that yields a <see cref="LinkInfo"/> instance.</returns>
    Task<LinkInfo> GetLinkInfoAsync(string senderAddress, string receiverAddress,
        CcuDeviceKind kind = CcuDeviceKind.HomeMatic);
}
