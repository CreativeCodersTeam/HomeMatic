using AwesomeAssertions;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Exporting;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.ShowDetails;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Device.ShowDetails;

public class ShowDeviceDetailsCommandTests
{
    private const string DeviceAddress = "00019F2999BE83";

    [Fact]
    public async Task ExecuteAsync_ParamSetWithError_PrintsWarningInsteadOfValues()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "SERVICE",
            Values = [],
            Error = "XML-RPC fault -321 (device not reachable (e.g. sleeping battery-powered device))"
        }));

        var options = new ShowDeviceDetailsOptions { Address = DeviceAddress };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should()
            .Contain("Values could not be read: XML-RPC fault -321");
    }

    [Fact]
    public async Task ExecuteAsync_ParamSetWithoutError_PrintsValues()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "MASTER",
            Values =
            [
                new ParamValueExportData { Key = "LONG_PRESS_TIME", Name = null, Value = 0.4 }
            ]
        }));

        var options = new ShowDeviceDetailsOptions { Address = DeviceAddress };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().Contain("LONG_PRESS_TIME").And.NotContain("could not be read");
    }

    [Fact]
    public async Task ExecuteAsync_WithParamSetsOption_ForwardsWhitelistToBuildAndExport()
    {
        // Arrange
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "master, values"
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - whitelist entries are split and trimmed, and reach both the build and the export options.
        sut.CapturedBuildOptions.Should().NotBeNull();
        sut.CapturedBuildOptions!.ParamSetWhitelist.Should().BeEquivalentTo("master", "values");
        sut.CapturedExportOptions.Should().NotBeNull();
        sut.CapturedExportOptions!.ParamSetWhitelist.Should().BeEquivalentTo("master", "values");
    }

    [Fact]
    public async Task ExecuteAsync_WithoutParamSetsOption_WhitelistIsNull()
    {
        // Arrange
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions { Address = DeviceAddress };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.CapturedBuildOptions.Should().NotBeNull();
        sut.CapturedBuildOptions!.ParamSetWhitelist.Should().BeNull();
        sut.CapturedExportOptions.Should().NotBeNull();
        sut.CapturedExportOptions!.ParamSetWhitelist.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ErroredParamSetFollowedByHealthyOne_SuppressesErroredValuesAndPrintsHealthyOnes()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(
            new ParamSetExportData
            {
                ParamSetKey = "SERVICE",
                Values =
                [
                    new ParamValueExportData { Key = "MUST_NOT_APPEAR", Name = null, Value = 1 }
                ],
                Error = "XML-RPC fault -321"
            },
            new ParamSetExportData
            {
                ParamSetKey = "MASTER",
                Values =
                [
                    new ParamValueExportData { Key = "LONG_PRESS_TIME", Name = null, Value = 0.4 }
                ]
            }));

        var options = new ShowDeviceDetailsOptions { Address = DeviceAddress };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - errored values are suppressed, but the loop continues with the next ParamSet.
        var output = sut.Output.ToString();
        output.Should().NotContain("MUST_NOT_APPEAR");
        output.Should().Contain("LONG_PRESS_TIME");
    }

    [Fact]
    public async Task ExecuteAsync_ErrorContainingMarkupCharacters_PrintsLiteralText()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "SERVICE",
            Values = [],
            Error = "fault [brackets] included"
        }));

        var options = new ShowDeviceDetailsOptions { Address = DeviceAddress };

        // Act
        var act = async () => await sut.Command.ExecuteAsync(options);

        // Assert - markup characters in the error must be escaped, not interpreted.
        await act.Should().NotThrowAsync();
        sut.Output.ToString().Should().Contain("fault [brackets] included");
    }

    [Fact]
    public async Task ExecuteAsync_WhitelistMatchesNothing_PrintsFilterWarning()
    {
        // Arrange - whitelist set, but the export contains no ParamSets at all.
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "MASTRE"
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().Contain("No ParamSets matched the --param-sets filter.");
    }

    [Fact]
    public async Task ExecuteAsync_WhitelistMatchesParamSets_DoesNotPrintFilterWarning()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "MASTER",
            Values = []
        }));
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "MASTER"
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().NotContain("No ParamSets matched");
    }

    [Fact]
    public async Task ExecuteAsync_NoWhitelistAndNoParamSets_DoesNotPrintFilterWarning()
    {
        // Arrange
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions { Address = DeviceAddress };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().NotContain("No ParamSets matched");
    }

    [Fact]
    public async Task ExecuteAsync_WhitelistSetButDeviceHasNoParamSetsAtAll_DoesNotPrintFilterWarning()
    {
        // Arrange - the device offers no ParamSets anywhere, so the filter did not remove anything.
        var sut = CreateSut(BuildExportData(paramSetKeys: []), snapshotParamSets: []);
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "MASTER"
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().NotContain("No ParamSets matched");
    }

    [Fact]
    public async Task ExecuteAsync_WithSkipServiceParamsOption_ForwardsFlagToBuildAndExport()
    {
        // Arrange
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - the flag must reach the fetch (build options) and the export options.
        sut.CapturedBuildOptions.Should().NotBeNull();
        sut.CapturedBuildOptions!.SkipServiceParamSet.Should().BeTrue();
        sut.CapturedExportOptions.Should().NotBeNull();
        sut.CapturedExportOptions!.SkipServiceParamSet.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSkipServiceParamsOption_FlagIsFalse()
    {
        // Arrange
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions { Address = DeviceAddress };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.CapturedBuildOptions.Should().NotBeNull();
        sut.CapturedBuildOptions!.SkipServiceParamSet.Should().BeFalse();
        sut.CapturedExportOptions.Should().NotBeNull();
        sut.CapturedExportOptions!.SkipServiceParamSet.Should().BeFalse();
    }

    [Theory]
    [InlineData("SERVICE")]
    [InlineData("service")]
    [InlineData("MASTER,SERVICE")]
    public async Task ExecuteAsync_SkipServiceParamsAndServiceInWhitelist_PrintsOverrideWarning(string paramSets)
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "MASTER",
            Values = []
        }));
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = paramSets,
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().Contain("--skip-service-params overrides SERVICE in --param-sets.");
    }

    [Fact]
    public async Task ExecuteAsync_SkipServiceParamsWithoutServiceInWhitelist_DoesNotPrintOverrideWarning()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "MASTER",
            Values = []
        }));
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "MASTER",
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - no contradiction, so no warning.
        sut.Output.ToString().Should().NotContain("overrides SERVICE");
    }

    [Fact]
    public async Task ExecuteAsync_ServiceInWhitelistWithoutSkipServiceParams_DoesNotPrintOverrideWarning()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "SERVICE",
            Values = []
        }));
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "SERVICE"
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().NotContain("overrides SERVICE");
    }

    [Fact]
    public async Task ExecuteAsync_SkipServiceParamsWithoutWhitelist_DoesNotPrintOverrideWarning()
    {
        // Arrange
        var sut = CreateSut(BuildExportData(new ParamSetExportData
        {
            ParamSetKey = "MASTER",
            Values = []
        }));
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - nothing to override without a --param-sets whitelist.
        sut.Output.ToString().Should().NotContain("overrides SERVICE");
    }

    [Fact]
    public async Task ExecuteAsync_SkipServiceParamsAndOnlyServiceInWhitelist_SuppressesFilterWarning()
    {
        // Arrange - the whitelist matches only SERVICE, which the skip then removes, so nothing is left to show.
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "SERVICE",
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - blaming the --param-sets filter would be wrong; the override warning already explains it.
        var output = sut.Output.ToString();
        output.Should().Contain("--skip-service-params overrides SERVICE in --param-sets.");
        output.Should().NotContain("No ParamSets matched");
    }

    [Fact]
    public async Task ExecuteAsync_SkipServiceParamsAndSurvivingWhitelistEntryMatchesNothing_PrintsBothWarnings()
    {
        // Arrange - SERVICE is overridden by the skip, but MASTER survives the skip and matches nothing.
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "MASTER,SERVICE",
            SkipServiceParamSet = true
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert - the override note must not swallow the legitimate filter warning for MASTER.
        var output = sut.Output.ToString();
        output.Should().Contain("--skip-service-params overrides SERVICE in --param-sets.");
        output.Should().Contain("No ParamSets matched the --param-sets filter.");
    }

    [Fact]
    public async Task ExecuteAsync_WhitelistMatchesNothingWithoutSkip_StillPrintsFilterWarning()
    {
        // Arrange - no contradiction, so the filter warning must not be suppressed.
        var sut = CreateSut(BuildExportData());
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "SERVICE",
            SkipServiceParamSet = false
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().Contain("No ParamSets matched the --param-sets filter.");
    }

    [Fact]
    public async Task ExecuteAsync_OnlyChannelsOfferParamSets_PrintsFilterWarning()
    {
        // Arrange - the device itself has no ParamSets, but a channel does, so the filter did remove something.
        var sut = CreateSut(BuildExportData(paramSetKeys: []), snapshotParamSets: [],
            snapshotChannelParamSets: ["VALUES"]);
        var options = new ShowDeviceDetailsOptions
        {
            Address = DeviceAddress,
            ParamSets = "MASTRE"
        };

        // Act
        await sut.Command.ExecuteAsync(options);

        // Assert
        sut.Output.ToString().Should().Contain("No ParamSets matched the --param-sets filter.");
    }

    private static DeviceExportData BuildExportData(params ParamSetExportData[] paramSets)
    {
        return BuildExportData(["MASTER", "SERVICE"], paramSets);
    }

    private static DeviceExportData BuildExportData(string[] paramSetKeys,
        params ParamSetExportData[] paramSets)
    {
        return new DeviceExportData
        {
            Name = "Wall Switch",
            Address = DeviceAddress,
            DeviceType = "HMIP-WRC2",
            ParamSetKeys = paramSetKeys,
            FirmwareVersion = "1.18.2",
            Ccu = "OG",
            ParamSetValues = paramSets,
            Channels = []
        };
    }

    private static SutContext CreateSut(DeviceExportData exportData, string[]? snapshotParamSets = null,
        string[]? snapshotChannelParamSets = null)
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
        var snapshot = CreateSnapshot(snapshotParamSets ?? ["MASTER", "SERVICE"], snapshotChannelParamSets ?? []);

        var multiCcuClient = A.Fake<IMultiCcuClient>();
        A.CallTo(() => multiCcuClient.GetCompleteDeviceAsync(DeviceAddress, A<CompleteCcuDeviceBuildOptions>._))
            .ReturnsLazily((string _, CompleteCcuDeviceBuildOptions buildOptions) =>
            {
                context.CapturedBuildOptions = buildOptions;
                return Task.FromResult(snapshot);
            });

        var deviceExporter = A.Fake<IDeviceExporter>();
        A.CallTo(() => deviceExporter.BuildExportData(A<ICompleteCcuDevice>._, A<DeviceExportOptions>._))
            .ReturnsLazily((ICompleteCcuDevice _, DeviceExportOptions exportOptions) =>
            {
                context.CapturedExportOptions = exportOptions;
                return exportData;
            });

        context.Command = new ShowDeviceDetailsCommand(console, multiCcuClient, deviceExporter);

        return context;
    }

    /// <summary>
    /// Builds the unfiltered device snapshot. The command reads its ParamSet keys from here to decide whether the
    /// device offers any ParamSets at all — the export data cannot answer that, because its key lists are filtered.
    /// </summary>
    private static ICompleteCcuDevice CreateSnapshot(string[] paramSets, string[] channelParamSets)
    {
        var deviceData = A.Fake<ICcuDeviceData>();
        A.CallTo(() => deviceData.ParamSets).Returns(paramSets);

        var snapshot = A.Fake<ICompleteCcuDevice>();
        A.CallTo(() => snapshot.DeviceData).Returns(deviceData);
        A.CallTo(() => snapshot.Channels).Returns(channelParamSets.Length == 0
            ? []
            : [CreateSnapshotChannel(channelParamSets)]);

        return snapshot;
    }

    private static ICompleteCcuDeviceChannel CreateSnapshotChannel(string[] paramSets)
    {
        var channelData = A.Fake<ICcuDeviceChannel>();
        A.CallTo(() => channelData.ParamSets).Returns(paramSets);

        var channel = A.Fake<ICompleteCcuDeviceChannel>();
        A.CallTo(() => channel.ChannelData).Returns(channelData);

        return channel;
    }

    private sealed class SutContext(StringWriter output)
    {
        public ShowDeviceDetailsCommand Command { get; set; } = null!;

        public StringWriter Output { get; } = output;

        public CompleteCcuDeviceBuildOptions? CapturedBuildOptions { get; set; }

        public DeviceExportOptions? CapturedExportOptions { get; set; }
    }
}
