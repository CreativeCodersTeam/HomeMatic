using System.Net;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public interface ICcuConnectionsStore
{
    Task<bool> AddConnectionAsync(CcuConnectionInfo connectionInfo);
    
    Task<bool> RemoveConnectionAsync(Uri ccuUrl);
    
    Task<bool> RemoveConnectionAsync(string name);
    
    Task<IReadOnlyCollection<CcuConnectionInfo>> GetConnectionsAsync();
    
    NetworkCredential GetCredentials(CcuConnectionInfo ccuConnectionInfo);
}