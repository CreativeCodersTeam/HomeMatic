using System.Net;
using System.Text.Json;
using CreativeCoders.Core;
using CreativeCoders.Core.IO;
using GitCredentialManager;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CcuConnectionsStore(IAnsiConsole console) : ICcuConnectionsStore
{
    private readonly IAnsiConsole _ansiConsole = Ensure.NotNull(console);

    private readonly ICredentialStore _credentialStore = CredentialManager.Create("HomeMatic");

    public async Task<bool> AddConnectionAsync(CcuConnectionInfo connectionInfo)
    {
        var connections = await GetConnectionsAsync().ConfigureAwait(false);

        if (connections.Any(x => x.Url == connectionInfo.Url || HasName(x, connectionInfo.Name)))
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

        return RemoveConnectionAsync(x => HasName(x, name));
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
            return [];
        }

        var stream = FileSys.File.OpenRead(GetConnectionsFileName());
        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer
                       .DeserializeAsync<IReadOnlyCollection<CcuConnectionInfo>>(stream)
                       .ConfigureAwait(false) ??
                   [];
        }
    }

    /// <inheritdoc />
    public async Task<CcuConnectionInfo?> FindConnectionAsync(string name)
    {
        Ensure.NotNull(name);

        var connections = await GetConnectionsAsync().ConfigureAwait(false);

        return connections.FirstOrDefault(x => HasName(x, name));
    }

    public IReadOnlyCollection<CcuConnectionInfo> GetConnections()
    {
        if (!FileSys.File.Exists(GetConnectionsFileName()))
        {
            return [];
        }

        var json = FileSys.File.ReadAllText(GetConnectionsFileName());

        return JsonSerializer.Deserialize<IReadOnlyCollection<CcuConnectionInfo>>(json) ?? [];
    }

    public NetworkCredential GetCredentials(CcuConnectionInfo ccuConnectionInfo)
    {
        var credential = _credentialStore.Get($"ccu://{ccuConnectionInfo.Url.Host}", null);

        if (credential is not null)
        {
            return new NetworkCredential(credential.Account, credential.Password);
        }

        _ansiConsole.MarkupLine($"Input login credentials for {ccuConnectionInfo.Url.Host}");

        var userName = _ansiConsole.Prompt(new TextPrompt<string>("User name: "));

        var password = _ansiConsole.Prompt(new TextPrompt<string>("Password: ") { IsSecret = true });

        _credentialStore.AddOrUpdate($"ccu://{ccuConnectionInfo.Url.Host}", userName, password);

        return new NetworkCredential(userName, password);
    }

    private static bool HasName(CcuConnectionInfo connection, string name)
    {
        return string.Equals(connection.Name, name, StringComparison.OrdinalIgnoreCase);
    }

    private Task SaveConnectionsAsync(IEnumerable<CcuConnectionInfo> connections)
    {
        EnsureConfigPath();

        var json = JsonSerializer.Serialize(connections,
            new JsonSerializerOptions(JsonSerializerDefaults.General) { WriteIndented = true });

        return FileSys.File.WriteAllTextAsync(GetConnectionsFileName(), json);
    }

    private static void EnsureConfigPath()
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
