using System.Text.Json;
using CreativeCoders.Core.IO;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CcuConnectionsStore : ICcuConnectionsStore
{
    public async Task AddConnectionAsync(CcuConnectionInfo connectionInfo)
    {
        await SaveConnectionsAsync(
                (await GetConnectionsAsync().ConfigureAwait(false))
                .Append(connectionInfo))
            .ConfigureAwait(false);
    }

    public async Task RemoveConnectionAsync(CcuConnectionInfo connectionInfo)
    {
        await SaveConnectionsAsync(
                (await GetConnectionsAsync().ConfigureAwait(false))
                .ExceptBy<CcuConnectionInfo, Uri>(
                    new[] { connectionInfo.Url }, x => x.Url))
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<CcuConnectionInfo>> GetConnectionsAsync()
    {
        if (!FileSys.File.Exists(GetConnectionsFileName()))
        {
            return Array.Empty<CcuConnectionInfo>();
        }
        
        var json = await FileSys.File.ReadAllTextAsync(GetConnectionsFileName())
            .ConfigureAwait(false);

        return JsonSerializer.Deserialize<IEnumerable<CcuConnectionInfo>>(json) ??
               Array.Empty<CcuConnectionInfo>();
    }
    
    private Task SaveConnectionsAsync(IEnumerable<CcuConnectionInfo> connections)
    {
        EnsureConfigPath();
        
        var json = JsonSerializer.Serialize(connections,
            new JsonSerializerOptions(JsonSerializerDefaults.General){WriteIndented = true});
        
        return FileSys.File.WriteAllTextAsync(GetConnectionsFileName(), json);
    }

    private void EnsureConfigPath()
    {
        FileSys.Directory.CreateDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            HomeMaticToolApp.ConfigFolderName));
    }
    
    private static string GetConnectionsFileName() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        HomeMaticToolApp.ConfigFolderName,
        HomeMaticToolApp.CcuConnectionsStoreFileName);
}