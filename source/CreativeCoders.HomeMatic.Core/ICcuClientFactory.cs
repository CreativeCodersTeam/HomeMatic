using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Creates <see cref="ICcuClient"/> instances configured for a specific CCU.
/// </summary>
public interface ICcuClientFactory
{
    /// <summary>
    /// Creates a new <see cref="ICcuClient"/> that connects to the specified CCU.
    /// </summary>
    /// <param name="ccuName">The logical name of the CCU.</param>
    /// <param name="deviceKinds">The device kinds the client should address.</param>
    /// <param name="host">The host name or IP address of the CCU.</param>
    /// <param name="userName">The user name used to authenticate against the CCU.</param>
    /// <param name="password">The password used to authenticate against the CCU.</param>
    /// <returns>A new <see cref="ICcuClient"/> instance.</returns>
    ICcuClient CreateClient(string ccuName, IEnumerable<CcuDeviceKind> deviceKinds, string host, string userName,
        string password);
}
