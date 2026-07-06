using System.IO.Abstractions;
using System.Net;
using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.FirmwareBackup;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Backup;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Backup;

public class BackupCcuCommandTests
{
    private const string ConnectionName = "test-ccu";

    [Fact]
    public async Task ExecuteAsync_SkipVerifyNotSet_PassesVerifyBackupTrueToFactory()
    {
        // Arrange
        var sut = CreateSut();
        var options = new BackupCcuOptions { Name = ConnectionName, OutputFile = "backup.sbk" };

        // Act
        var result = await sut.Command.ExecuteAsync(options);

        // Assert
        result.Should().Be(CommandResult.Success);
        sut.CapturedOptions.Should().NotBeNull();
        sut.CapturedOptions!.VerifyBackup.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_SkipVerifySet_PassesVerifyBackupFalseToFactory()
    {
        // Arrange
        var sut = CreateSut();
        var options = new BackupCcuOptions
        {
            Name = ConnectionName,
            OutputFile = "backup.sbk",
            SkipVerify = true
        };

        // Act
        var result = await sut.Command.ExecuteAsync(options);

        // Assert
        result.Should().Be(CommandResult.Success);
        sut.CapturedOptions.Should().NotBeNull();
        sut.CapturedOptions!.VerifyBackup.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_SkipVerifySet_WritesWarning()
    {
        // Arrange
        var sut = CreateSut();
        var options = new BackupCcuOptions
        {
            Name = ConnectionName,
            OutputFile = "backup.sbk",
            SkipVerify = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().Contain("Backup verification is skipped.");
    }

    [Fact]
    public async Task ExecuteAsync_SkipVerifyNotSet_DoesNotWriteWarning()
    {
        // Arrange
        var sut = CreateSut();
        var options = new BackupCcuOptions { Name = ConnectionName, OutputFile = "backup.sbk" };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().NotContain("Backup verification is skipped.");
    }

    [Fact]
    public async Task ExecuteAsync_SkipVerifySetAndBackupFails_WritesWarningAndErrorAndReturnsError()
    {
        // Arrange
        var sut = CreateSut();
        A.CallTo(() => sut.Client.CreateBackupToFileAsync(A<string>._, A<CancellationToken>._))
            .Throws(new FirmwareBackupException("backup broken"));
        var options = new BackupCcuOptions
        {
            Name = ConnectionName,
            OutputFile = "backup.sbk",
            SkipVerify = true
        };

        // Act
        var result = await sut.Command.ExecuteAsync(options);

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.Output.ToString().Should().Contain("Backup verification is skipped.");
        sut.Output.ToString().Should().Contain("backup broken");
    }

    [Fact]
    public async Task ExecuteAsync_SkipVerifySetAndConnectionNotFound_ReturnsErrorWithoutWarning()
    {
        // Arrange
        var sut = CreateSut();
        var options = new BackupCcuOptions
        {
            Name = "unknown-ccu",
            OutputFile = "backup.sbk",
            SkipVerify = true
        };

        // Act
        var result = await sut.Command.ExecuteAsync(options);

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.Output.ToString().Should().NotContain("Backup verification is skipped.");
    }

    private static SutContext CreateSut()
    {
        var output = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Interactive = InteractionSupport.No,
            Out = new AnsiConsoleOutput(output)
        });

        var connection = new CcuConnectionInfo(new Uri("https://ccu.example.local"), ConnectionName);

        var connectionsStore = A.Fake<ICcuConnectionsStore>();
        A.CallTo(() => connectionsStore.GetConnectionsAsync())
            .Returns(new[] { connection });
        A.CallTo(() => connectionsStore.GetCredentials(connection))
            .Returns(new NetworkCredential("Admin", "secret"));

        var client = A.Fake<IFirmwareBackupClient>();
        A.CallTo(() => client.CreateBackupToFileAsync(A<string>._, A<CancellationToken>._))
            .Returns("/backups/ccu_backup.sbk");

        var context = new SutContext(output);

        var factory = A.Fake<IFirmwareBackupClientFactory>();
        A.CallTo(() => factory.Create(A<FirmwareBackupOptions>._))
            .ReturnsLazily((FirmwareBackupOptions backupOptions) =>
            {
                context.CapturedOptions = backupOptions;
                return client;
            });

        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.Path.GetFullPath(A<string>._))
            .ReturnsLazily((string path) => "/backups/" + path);
        A.CallTo(() => fileSystem.Path.GetDirectoryName(A<string>._))
            .Returns("/backups");

        context.Client = client;
        context.Command = new BackupCcuCommand(console, connectionsStore, factory, fileSystem);

        return context;
    }

    private sealed class SutContext(StringWriter output)
    {
        public BackupCcuCommand Command { get; set; } = null!;

        public IFirmwareBackupClient Client { get; set; } = null!;

        public StringWriter Output { get; } = output;

        public FirmwareBackupOptions? CapturedOptions { get; set; }
    }
}
