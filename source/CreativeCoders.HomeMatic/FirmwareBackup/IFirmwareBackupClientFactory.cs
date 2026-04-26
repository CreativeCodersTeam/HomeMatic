using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Factory for creating <see cref="IFirmwareBackupClient"/> instances configured for a specific CCU.
/// </summary>
[PublicAPI]
public interface IFirmwareBackupClientFactory
{
    /// <summary>
    /// Creates a new <see cref="IFirmwareBackupClient"/> using the given options.
    /// </summary>
    /// <param name="options">Connection and behavior options for the CCU.</param>
    /// <returns>A new <see cref="IFirmwareBackupClient"/> instance.</returns>
    IFirmwareBackupClient Create(FirmwareBackupOptions options);
}
