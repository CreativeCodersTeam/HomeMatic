using System.Net;
using System.Text.Json;
using CreativeCoders.Core;
using CreativeCoders.Core.IO;
using GitCredentialManager;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CcuConnectionsStore : ICcuConnectionsStore
{
    private readonly IAnsiConsole _console;
    
    private readonly ICredentialStore _credentialStore = CredentialManager.Create("HomeMatic");

    public CcuConnectionsStore(IAnsiConsole console)
    {
        _console = Ensure.NotNull(console);
    }
    
    public async Task<bool> AddConnectionAsync(CcuConnectionInfo connectionInfo)
    {
        var connections = await GetConnectionsAsync().ConfigureAwait(false);
        
        if (connections.Any(x => x.Url == connectionInfo.Url || x.Name == connectionInfo.Name))
        {
            return false;
        }
        
        await SaveConnectionsAsync(
                connections.Append(connectionInfo))
            .ConfigureAwait(false);

        return true;
    }

    public Task<bool> RemoveConnectionAsync(Uri ccuUrl)
    {
        Ensure.NotNull(ccuUrl);
        
        return RemoveConnectionAsync(x => x.Url.Equals(ccuUrl));
    }
    
    public Task<bool> RemoveConnectionAsync(string name)
    {
        Ensure.NotNull(name);
        
        return RemoveConnectionAsync(x => x.Name == name);
    }
    
    private async Task<bool> RemoveConnectionAsync(Func<CcuConnectionInfo, bool> predicate)
    {
        var connections = await GetConnectionsAsync().ConfigureAwait(false);
        
        if (connections.All(x => !predicate(x)))
        {
            return false;
        }
        
        await SaveConnectionsAsync(
                (await GetConnectionsAsync().ConfigureAwait(false))
                .Where(x => !predicate(x)))
            .ConfigureAwait(false);

        return true;
    }

    public async Task<IReadOnlyCollection<CcuConnectionInfo>> GetConnectionsAsync()
    {
        if (!FileSys.File.Exists(GetConnectionsFileName()))
        {
            return Array.Empty<CcuConnectionInfo>();
        }
        
        var json = await FileSys.File.ReadAllTextAsync(GetConnectionsFileName())
            .ConfigureAwait(false);

        return JsonSerializer.Deserialize<IReadOnlyCollection<CcuConnectionInfo>>(json) ??
               Array.Empty<CcuConnectionInfo>();
    }

    public NetworkCredential GetCredentials(CcuConnectionInfo ccuConnectionInfo)
    {
        var credential = _credentialStore.Get($"ccu://{ccuConnectionInfo.Url.Host}", null);

        if (credential is not null)
        {
            return new NetworkCredential(credential.Account, credential.Password);
        }
        
        _console.MarkupLine($"Input login credentials for {ccuConnectionInfo.Url.Host}");
        
        var userName = _console.Prompt(new TextPrompt<string>("User name: "));
            
        var password = _console.Prompt(new TextPrompt<string>("Password: "){IsSecret = true});
            
        _credentialStore.AddOrUpdate($"ccu://{ccuConnectionInfo.Url.Host}", userName, password);

        return new NetworkCredential(userName, password);
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