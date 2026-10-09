using System.Net;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public interface ICcuConnectionsStore
{
    Task<bool> AddConnectionAsync(CcuConnectionInfo connectionInfo);

    Task<bool> RemoveConnectionAsync(Uri ccuUrl);

    Task<bool> RemoveConnectionAsync(string name);

    Task<IReadOnlyCollection<CcuConnectionInfo>> GetConnectionsAsync();

    /// <summary>
    /// Finds the stored connection with the specified name.
    /// </summary>
    /// <param name="name">The name of the connection. The name is compared ignoring case.</param>
    /// <returns>
    /// A task whose result contains the first connection with the specified name, or <see langword="null"/> if no
    /// connection has that name.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    Task<CcuConnectionInfo?> FindConnectionAsync(string name);

    IReadOnlyCollection<CcuConnectionInfo> GetConnections();

    NetworkCredential GetCredentials(CcuConnectionInfo ccuConnectionInfo);
}
