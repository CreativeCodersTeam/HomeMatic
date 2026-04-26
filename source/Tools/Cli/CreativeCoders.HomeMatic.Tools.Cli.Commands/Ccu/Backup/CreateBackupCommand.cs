using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Backup;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Backup;

/// <summary>
/// CLI command that creates a system backup of a CCU.
/// </summary>
[UsedImplicitly]
[CliCommand([CcuCommandGroup.Name, "backup"], Description = "Create a backup of a CCU")]
public class CreateBackupCommand(
    IAnsiConsole console,
    ICcuConnectionsStore ccuConnectionsStore,
    ICcuBackupServiceBuilder ccuBackupServiceBuilder)
    : ICliCommand<CreateBackupOptions>
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly ICcuConnectionsStore _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);

    private readonly ICcuBackupServiceBuilder _ccuBackupServiceBuilder = Ensure.NotNull(ccuBackupServiceBuilder);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(CreateBackupOptions options)
    {
        var connections = await _ccuConnectionsStore.GetConnectionsAsync().ConfigureAwait(false);

        var connection = connections.FirstOrDefault(
            x => string.Equals(x.Name, options.ConnectionName, StringComparison.OrdinalIgnoreCase));

        if (connection is null)
        {
            _console.MarkupLine($"[bold red]Connection '{options.ConnectionName}' not found[/]");
            return -1;
        }

        var credentials = _ccuConnectionsStore.GetCredentials(connection);

        var backupService = _ccuBackupServiceBuilder
            .ForHost(connection.Url.Host)
            .WithCredentials(credentials)
            .Build();

        var outputFilePath = options.OutputFilePath
                             ?? $"backup_{connection.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.tar.gz";

        await _console.Status()
            .StartAsync($"Creating backup for '{connection.Name}'...", async _ =>
            {
                await backupService.SaveBackupAsync(outputFilePath).ConfigureAwait(false);
            })
            .ConfigureAwait(false);

        _console.MarkupLine($"[bold green]Backup saved to '{outputFilePath}'[/]");

        return CommandResult.Success;
    }
}
