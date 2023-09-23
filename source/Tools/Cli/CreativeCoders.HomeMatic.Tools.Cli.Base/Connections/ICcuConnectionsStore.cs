namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public interface ICcuConnectionsStore
{
    Task AddConnectionAsync(CcuConnectionInfo connectionInfo);
    
    Task RemoveConnectionAsync(CcuConnectionInfo connectionInfo);
    
    Task<IEnumerable<CcuConnectionInfo>> GetConnectionsAsync();
}