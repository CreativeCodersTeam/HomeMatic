using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.FirmwareBackup;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Backup;

[UsedImplicitly]
[CliCommand([CcuCommandGroup.Name, "backup"], Description = "Create a firmware backup of a configured CCU")]
public class BackupCcuCommand(
    IAnsiConsole console,
    ICcuConnectionsStore ccuConnectionsStore,
    IFirmwareBackupClientFactory firmwareBackupClientFactory)
    : ICliCommand<BackupCcuOptions>
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly ICcuConnectionsStore _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);

    private readonly IFirmwareBackupClientFactory _firmwareBackupClientFactory =
        Ensure.NotNull(firmwareBackupClientFactory);

    public async Task<CommandResult> ExecuteAsync(BackupCcuOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Name))
        {
            _console.MarkupLine("[bold italic red3]A CCU connection name is required.[/]");
            return -1;
        }

        var connections = await _ccuConnectionsStore.GetConnectionsAsync().ConfigureAwait(false);

        var connection = connections
            .FirstOrDefault(x => string.Equals(x.Name, options.Name, StringComparison.OrdinalIgnoreCase));

        if (connection is null)
        {
            _console.MarkupLine(
                $"[bold italic red3]No CCU connection named '{Markup.Escape(options.Name)}' found.[/]");
            return -1;
        }

        var credential = _ccuConnectionsStore.GetCredentials(connection);

        var backupOptions = new FirmwareBackupOptions(connection.Url, credential);
        var client = _firmwareBackupClientFactory.Create(backupOptions);

        var outputDirectory = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Environment.CurrentDirectory
            : options.OutputDirectory;

        Directory.CreateDirectory(outputDirectory);

        var fileName = BuildBackupFileName(connection.Name);
        var targetPath = Path.Combine(outputDirectory, fileName);

        _console.MarkupLine(
            $"Creating firmware backup of CCU [bold]{Markup.Escape(connection.Name)}[/] " +
            $"([italic]{Markup.Escape(connection.Url.ToString())}[/])...");

        try
        {
            var writtenPath = await client.CreateBackupToFileAsync(targetPath).ConfigureAwait(false);

            _console.MarkupLine($"[bold lime]Backup created:[/] {Markup.Escape(writtenPath)}");
            return CommandResult.Success;
        }
        catch (FirmwareBackupException ex)
        {
            _console.MarkupLine($"[bold italic red3]Backup failed: {Markup.Escape(ex.Message)}[/]");
            return -1;
        }
    }

    private static string BuildBackupFileName(string ccuName)
    {
        var safeName = MakeFileNameSafe(ccuName);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"{safeName}_{timestamp}.sbk";
    }

    private static string MakeFileNameSafe(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "ccu" : sanitized;
    }
}
