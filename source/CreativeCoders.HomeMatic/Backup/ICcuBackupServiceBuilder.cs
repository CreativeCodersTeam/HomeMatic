using System.Net;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Backup;

/// <summary>
/// Builds <see cref="ICcuBackupService"/> instances configured for a specific CCU.
/// </summary>
[PublicAPI]
public interface ICcuBackupServiceBuilder
{
    /// <summary>
    /// Sets the host name or IP address of the CCU to back up.
    /// </summary>
    /// <param name="host">The host name or IP address of the CCU.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    ICcuBackupServiceBuilder ForHost(string host);

    /// <summary>
    /// Sets the credentials used to authenticate against the CCU.
    /// </summary>
    /// <param name="credential">The network credentials containing user name and password.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    ICcuBackupServiceBuilder WithCredentials(NetworkCredential credential);

    /// <summary>
    /// Creates a new <see cref="ICcuBackupService"/> from the current builder configuration.
    /// </summary>
    /// <returns>A configured <see cref="ICcuBackupService"/> instance.</returns>
    ICcuBackupService Build();
}
