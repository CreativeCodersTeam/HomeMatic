using AwesomeAssertions;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Exporting;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.Export;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Device.Export;

public class ExportDevicesCommandTests : IDisposable
{
    private const string DeviceAddress = "00019F2999BE83";

    private readonly string _outputDirectory =
        Path.Combine(Path.GetTempPath(), $"export-devices-tests-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ExecuteAsync_SkipServiceParamSetSet_ForwardsSkipToBuildAndExportOptions()
    {
        // Arrange
        var sut = CreateSut();
        var options = new ExportDevicesOptions
        {
            Address = DeviceAddress,
            OutputFileName = CreateOutputFileName(),
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - the flag must reach the CCU fetch and the export data building.
        sut.CapturedBuildOptions.Should().NotBeNull();
        sut.CapturedBuildOptions!.SkipServiceParamSet.Should().BeTrue();
        sut.CapturedExportOptions.Should().NotBeNull();
        sut.CapturedExportOptions!.SkipServiceParamSet.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_SkipServiceParamSetNotSet_BuildAndExportOptionsDoNotSkipService()
    {
        // Arrange
        var sut = CreateSut();
        var options = new ExportDevicesOptions
        {
            Address = DeviceAddress,
            OutputFileName = CreateOutputFileName()
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.CapturedBuildOptions.Should().NotBeNull();
        sut.CapturedBuildOptions!.SkipServiceParamSet.Should().BeFalse();
        sut.CapturedExportOptions.Should().NotBeNull();
        sut.CapturedExportOptions!.SkipServiceParamSet.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithAnyOptions_ExportOptionsWriteIndentedIsSet()
    {
        // Arrange
        var sut = CreateSut();
        var options = new ExportDevicesOptions
        {
            Address = DeviceAddress,
            OutputFileName = CreateOutputFileName(),
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.CapturedExportOptions.Should().NotBeNull();
        sut.CapturedExportOptions!.WriteIndented.Should().BeTrue();
    }

    private string CreateOutputFileName()
    {
        Directory.CreateDirectory(_outputDirectory);

        return Path.Combine(_outputDirectory, "device.json");
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

        var context = new SutContext(output);

        var multiCcuClient = A.Fake<IMultiCcuClient>();
        A.CallTo(() => multiCcuClient.GetCompleteDeviceAsync(DeviceAddress, A<CompleteCcuDeviceBuildOptions>._))
            .ReturnsLazily((string _, CompleteCcuDeviceBuildOptions buildOptions) =>
            {
                context.CapturedBuildOptions = buildOptions;
                return Task.FromResult(A.Fake<ICompleteCcuDevice>());
            });

        var deviceExporter = A.Fake<IDeviceExporter>();
        A.CallTo(() => deviceExporter.BuildExportData(A<ICompleteCcuDevice>._, A<DeviceExportOptions>._))
            .ReturnsLazily((ICompleteCcuDevice _, DeviceExportOptions exportOptions) =>
            {
                context.CapturedExportOptions = exportOptions;
                return CreateExportData();
            });

        context.Command = new ExportDevicesCommand(console, multiCcuClient, deviceExporter);

        return context;
    }

    private static DeviceExportData CreateExportData()
    {
        return new DeviceExportData
        {
            Name = "Wall Switch",
            Address = DeviceAddress,
            DeviceType = "HMIP-WRC2",
            ParamSetKeys = ["MASTER"],
            FirmwareVersion = "1.18.2",
            Ccu = "OG",
            ParamSetValues = [],
            Channels = []
        };
    }

    private sealed class SutContext(StringWriter output)
    {
        public ExportDevicesCommand Command { get; set; } = null!;

        public StringWriter Output { get; } = output;

        public CompleteCcuDeviceBuildOptions? CapturedBuildOptions { get; set; }

        public DeviceExportOptions? CapturedExportOptions { get; set; }
    }
}
