using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Maintains a mapping between HomeMatic device addresses and the <see cref="ICcuClient"/> that owns them.
/// </summary>
/// <remarks>
/// The routing table enables <see cref="IMultiCcuClient"/> implementations to avoid querying every configured
/// CCU for each per-device call. Implementations must be safe for concurrent use.
/// </remarks>
public interface ICcuRoutingTable
{
    /// <summary>
    /// Attempts to resolve the <see cref="ICcuClient"/> that owns the device with the given address.
    /// </summary>
    /// <param name="address">The device address (without channel suffix).</param>
    /// <param name="client">When this method returns, contains the mapped client if found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a mapping was found; otherwise, <see langword="false"/>.</returns>
    bool TryGetClient(string address, out ICcuClient? client);

    /// <summary>
    /// Registers a mapping from the given device address to the specified <see cref="ICcuClient"/>.
    /// </summary>
    /// <param name="address">The device address (without channel suffix).</param>
    /// <param name="client">The owning CCU client.</param>
    void Register(string address, ICcuClient client);

    /// <summary>
    /// Registers multiple address-to-client mappings in one call.
    /// </summary>
    /// <param name="entries">The mappings to register.</param>
    void Register(IEnumerable<KeyValuePair<string, ICcuClient>> entries);

    /// <summary>
    /// Removes the mapping for the given device address if present.
    /// </summary>
    /// <param name="address">The device address (without channel suffix).</param>
    void Invalidate(string address);

    /// <summary>
    /// Removes all mappings from the routing table.
    /// </summary>
    void Clear();
}
