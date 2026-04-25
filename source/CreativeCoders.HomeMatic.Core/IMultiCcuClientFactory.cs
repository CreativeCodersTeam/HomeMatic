using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Builds an <see cref="IMultiCcuClient"/> by aggregating one or more individual CCU configurations.
/// </summary>
public interface IMultiCcuClientFactory
{
    /// <summary>
    /// Adds a CCU to the builder configuration.
    /// </summary>
    /// <param name="ccuName">The logical name of the CCU.</param>
    /// <param name="host">The host name or IP address of the CCU.</param>
    /// <param name="userName">The user name used to authenticate against the CCU.</param>
    /// <param name="password">The password used to authenticate against the CCU.</param>
    /// <param name="deviceKinds">The device kinds the CCU should serve.</param>
    /// <returns>The same <see cref="IMultiCcuClientFactory"/> instance, to allow chaining calls.</returns>
    IMultiCcuClientFactory AddCcu(string ccuName, string host, string userName, string password,
        params CcuDeviceKind[] deviceKinds);

    /// <summary>
    /// Builds an <see cref="IMultiCcuClient"/> from the previously added CCU configurations.
    /// </summary>
    /// <returns>A new <see cref="IMultiCcuClient"/> instance.</returns>
    IMultiCcuClient Build();
}
