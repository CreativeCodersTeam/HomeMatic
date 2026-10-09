using System.Globalization;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;
using CreativeCoders.HomeMatic.XmlRpc;
using Spectre.Console;
using Spectre.Console.Testing;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

public class CcuEventLineFormatterTests
{
    private const string DeviceAddress = "000A1B2C3D4E5F";

    private const string ChannelAddress = DeviceAddress + ":1";

    private static readonly DateTimeOffset ReceivedAt =
        new(2026, 10, 6, 21, 15, 3, 412, TimeSpan.FromHours(2));

    private static readonly DeviceNameDirectory Names = new(new Dictionary<string, string>
    {
        [DeviceAddress] = "Living room light"
    });

    [Fact]
    public void Format_KnownChannel_ReturnsLineInContractFormat()
    {
        // Arrange
        var record = CreateRecord(true, CcuDeviceKind.HomeMaticIp);

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Be(
            "21:15:03.412  HmIP-RF       000A1B2C3D4E5F:1  Living room light (Channel 1)  STATE = true");
    }

    [Fact]
    public void Format_ReceivedAt_StartsWithTimeInHoursMinutesSecondsMilliseconds()
    {
        // Arrange
        var record = CreateRecord(true);

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().StartWith("21:15:03.412  ");
    }

    [Theory]
    [InlineData(CcuDeviceKind.HomeMatic, "BidCos-RF")]
    [InlineData(CcuDeviceKind.HomeMaticIp, "HmIP-RF")]
    [InlineData(CcuDeviceKind.HomeMaticWired, "BidCos-Wired")]
    public void InterfaceLabel_SupportedKind_ReturnsCcuInterfaceName(CcuDeviceKind kind, string expectedLabel)
    {
        // Act
        var label = CcuEventLineFormatter.InterfaceLabel(kind);

        // Assert
        label.Should().Be(expectedLabel);
    }

    [Theory]
    [InlineData(CcuDeviceKind.HomeMatic, "BidCos-RF")]
    [InlineData(CcuDeviceKind.HomeMaticIp, "HmIP-RF")]
    [InlineData(CcuDeviceKind.HomeMaticWired, "BidCos-Wired")]
    public void Format_Interface_ContainsInterfaceLabel(CcuDeviceKind kind, string expectedLabel)
    {
        // Arrange
        var record = CreateRecord(true, kind);

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain($"  {expectedLabel} ");
    }

    [Theory]
    [InlineData(true, "STATE = true")]
    [InlineData(false, "STATE = false")]
    [InlineData(42, "STATE = 42")]
    [InlineData(21.5, "STATE = 21.5")]
    [InlineData("Küche", "STATE = \"Küche\"")]
    [InlineData("", "STATE = <empty>")]
    [InlineData(null, "STATE = <empty>")]
    public void Format_Value_FormatsValueByType(object? value, string expectedEnding)
    {
        // Arrange
        var record = CreateRecord(value);

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().EndWith(expectedEnding);
    }

    [Fact]
    public void Format_DoubleValueWithGermanCulture_UsesInvariantCulture()
    {
        // Arrange
        var record = CreateRecord(21.5);
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");

        try
        {
            // Act
            var line = CcuEventLineFormatter.Format(record, Names);

            // Assert
            line.Should().EndWith("STATE = 21.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Format_AddressNotInDirectory_ShowsUnknownName()
    {
        // Arrange
        var record = CreateRecord(true) with { Address = "FFFFFFFFFFFFFF:3" };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain("  FFFFFFFFFFFFFF:3  <unknown>  STATE = true");
    }

    [Fact]
    public void Format_DeviceNameInDirectory_ShowsDeviceNameWithChannel()
    {
        // Arrange
        var names = new DeviceNameDirectory(new Dictionary<string, string>
        {
            [DeviceAddress] = "Device",
            [ChannelAddress] = "HmIP-BSM 000A1B2C3D4E5F:1"
        });
        var record = CreateRecord(true);

        // Act
        var line = CcuEventLineFormatter.Format(record, names);

        // Assert
        line.Should().Contain($"  {ChannelAddress}  Device (Channel 1)  STATE = true");
    }

    [Theory]
    [InlineData(DeviceAddress + ":0", "Living room light (Channel 0)")]
    [InlineData(DeviceAddress + ":4", "Living room light (Channel 4)")]
    [InlineData(DeviceAddress + ":12", "Living room light (Channel 12)")]
    public void Format_ChannelAddressOfKnownDevice_ShowsDeviceNameWithChannel(string address, string expectedName)
    {
        // Arrange
        var record = CreateRecord(true) with { Address = address };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain($"  {address}  {expectedName}  STATE = true");
    }

    [Fact]
    public void Format_LowerCaseChannelAddressOfKnownDevice_ShowsDeviceNameWithChannel()
    {
        // Arrange
        var address = ChannelAddress.ToLowerInvariant();
        var record = CreateRecord(true) with { Address = address };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain($"  {address}  Living room light (Channel 1)  STATE = true");
    }

    [Theory]
    [InlineData(DeviceAddress + ":[x]", "Living room light (Channel [x])")]
    [InlineData(DeviceAddress + ":1\n", @"Living room light (Channel 1\x0A)")]
    public void Format_ChannelPartWithMarkupOrControlCharacters_KeepsMarkupAndEscapesControlCharacters(string address,
        string expectedName)
    {
        // Arrange
        var record = CreateRecord(true) with { Address = address };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain($"  {expectedName}  STATE = true");
    }

    [Fact]
    public void Format_DeviceAddressWithoutChannel_ShowsDeviceNameOnly()
    {
        // Arrange
        var record = CreateRecord(true) with { Address = DeviceAddress };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain($"  {DeviceAddress}  Living room light  STATE = true");
    }

    [Fact]
    public void Format_AddressWithSeveralSeparators_ShowsEverythingAfterFirstSeparatorAsChannel()
    {
        // Arrange
        var record = CreateRecord(true) with { Address = DeviceAddress + ":1:2" };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain($"  {DeviceAddress}:1:2  Living room light (Channel 1:2)  STATE = true");
    }

    [Fact]
    public void Format_AddressWithTrailingSeparator_ShowsEmptyChannel()
    {
        // Arrange
        var record = CreateRecord(true) with { Address = DeviceAddress + ":" };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Contain($"  {DeviceAddress}:  Living room light (Channel )  STATE = true");
    }

    [Fact]
    public void Format_MorningTimeWithZeroMilliseconds_PadsWithZeros()
    {
        // Arrange
        var record = CreateRecord(true) with { ReceivedAt = new DateTimeOffset(2026, 10, 6, 9, 5, 7, TimeSpan.Zero) };

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().StartWith("09:05:07.000  ");
    }

    [Fact]
    public void InterfaceLabel_UnsupportedKind_ReturnsKindName()
    {
        // Act
        var label = CcuEventLineFormatter.InterfaceLabel(CcuDeviceKind.Coupled);

        // Assert
        label.Should().Be("Coupled");
    }

    [Fact]
    public void Format_NamesUnavailable_ShowsNotAvailableName()
    {
        // Arrange
        var record = CreateRecord(true);

        // Act
        var line = CcuEventLineFormatter.Format(record, DeviceNameDirectory.Unavailable);

        // Assert
        line.Should().Contain($"  {ChannelAddress}  <n/a>  STATE = true");
    }

    [Fact]
    public void Format_NameWithMarkupCharacters_KeepsMarkupCharactersUnchanged()
    {
        // Arrange
        var names = new DeviceNameDirectory(new Dictionary<string, string> { [DeviceAddress] = "[Test]" });
        var record = CreateRecord(true);

        // Act
        var line = CcuEventLineFormatter.Format(record, names);

        // Assert
        line.Should().Contain("  [Test] (Channel 1)  ");
    }

    [Fact]
    public void Format_StringValueWithMarkupCharacters_KeepsMarkupCharactersUnchanged()
    {
        // Arrange
        var record = CreateRecord("[red]");

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().EndWith("STATE = \"[red]\"");
    }

    [Theory]
    [InlineData("Line\rbreak", @"Line\x0Dbreak")]
    [InlineData("Line\nbreak", @"Line\x0Abreak")]
    [InlineData("Esc\u001b[2Jape", @"Esc\x1B[2Jape")]
    [InlineData("Tab\tbed", @"Tab\x09bed")]
    public void Format_NameWithControlCharacters_ReplacesControlCharactersWithHexEscapes(string name,
        string expectedName)
    {
        // Arrange
        var names = new DeviceNameDirectory(new Dictionary<string, string> { [DeviceAddress] = name });
        var record = CreateRecord(true);

        // Act
        var line = CcuEventLineFormatter.Format(record, names);

        // Assert
        line.Should().Be(
            $"21:15:03.412  HmIP-RF       {ChannelAddress}  {expectedName} (Channel 1)  STATE = true");
    }

    [Theory]
    [InlineData("Line\rbreak", @"STATE = ""Line\x0Dbreak""")]
    [InlineData("Line\nbreak", @"STATE = ""Line\x0Abreak""")]
    [InlineData("Esc\u001b[2Jape", @"STATE = ""Esc\x1B[2Jape""")]
    [InlineData("Tab\tbed", @"STATE = ""Tab\x09bed""")]
    [InlineData("Del\u007fete", @"STATE = ""Del\x7Fete""")]
    [InlineData("Next\u0085line", @"STATE = ""Next\x85line""")]
    [InlineData("Csi\u009b2J", @"STATE = ""Csi\x9B2J""")]
    public void Format_StringValueWithControlCharacters_ReplacesControlCharactersWithHexEscapes(string value,
        string expectedEnding)
    {
        // Arrange
        var record = CreateRecord(value);

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().EndWith(expectedEnding);
    }

    [Fact]
    public void Format_AddressAndValueKeyWithControlCharacters_ReplacesControlCharactersWithHexEscapes()
    {
        // Arrange
        var record = new CcuEventRecord(ReceivedAt, CcuDeviceKind.HomeMaticIp, "ADDR\n:1", "STA\rTE", true);

        // Act
        var line = CcuEventLineFormatter.Format(record, Names);

        // Assert
        line.Should().Be(@"21:15:03.412  HmIP-RF       ADDR\x0A:1  <unknown>  STA\x0DTE = true");
    }

    [Fact]
    public void FormatMarkup_KnownChannel_ReturnsStyledLine()
    {
        // Arrange
        var record = CreateRecord(true);

        // Act
        var markup = CcuEventLineFormatter.FormatMarkup(record, Names);

        // Assert
        markup.Should().Be(
            "[grey]21:15:03.412[/]  [grey]HmIP-RF     [/]  [grey]000A1B2C3D4E5F:1[/]  " +
            "[bold]Living room light (Channel 1)[/]  [italic]STATE[/] = [bold lime]true[/]");
    }

    [Theory]
    [InlineData(true, "[bold lime]true[/]")]
    [InlineData(false, "[bold red]false[/]")]
    [InlineData(42, "[bold aqua]42[/]")]
    [InlineData(21.5, "[bold aqua]21.5[/]")]
    [InlineData("Küche", "[bold yellow]\"Küche\"[/]")]
    [InlineData("", "[grey italic]<empty>[/]")]
    [InlineData(null, "[grey italic]<empty>[/]")]
    public void FormatMarkup_Value_StylesValueByType(object? value, string expectedEnding)
    {
        // Arrange
        var record = CreateRecord(value);

        // Act
        var markup = CcuEventLineFormatter.FormatMarkup(record, Names);

        // Assert
        markup.Should().EndWith($"[italic]STATE[/] = {expectedEnding}");
    }

    [Fact]
    public void FormatMarkup_AddressNotInDirectory_ShowsUnknownNameAsPlaceholder()
    {
        // Arrange
        var record = CreateRecord(true) with { Address = "FFFFFFFFFFFFFF:3" };

        // Act
        var markup = CcuEventLineFormatter.FormatMarkup(record, Names);

        // Assert
        markup.Should().Contain("  [grey italic]<unknown>[/]  ");
    }

    [Fact]
    public void FormatMarkup_NamesUnavailable_ShowsNotAvailableNameAsPlaceholder()
    {
        // Arrange
        var record = CreateRecord(true);

        // Act
        var markup = CcuEventLineFormatter.FormatMarkup(record, DeviceNameDirectory.Unavailable);

        // Assert
        markup.Should().Contain("  [grey italic]<n/a>[/]  ");
    }

    [Fact]
    public void FormatMarkup_NameAndValueWithMarkupCharacters_EscapesMarkupCharacters()
    {
        // Arrange
        var names = new DeviceNameDirectory(new Dictionary<string, string> { [DeviceAddress] = "[Test]" });
        var record = CreateRecord("[red]");

        // Act
        var markup = CcuEventLineFormatter.FormatMarkup(record, names);

        // Assert
        markup.Should().Contain("  [bold][[Test]] (Channel 1)[/]  ");
        markup.Should().EndWith("[bold yellow]\"[[red]]\"[/]");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(0.5)]
    [InlineData("[red]")]
    [InlineData("[/]")]
    [InlineData("Esc\u001b[2Jape")]
    [InlineData("")]
    [InlineData(null)]
    public void FormatMarkup_RenderedWithoutAnsi_PrintsSameTextAsFormat(object? value)
    {
        // Arrange
        var names = new DeviceNameDirectory(new Dictionary<string, string> { [DeviceAddress] = "[b]Name[/b]" });
        var record = new CcuEventRecord(ReceivedAt, CcuDeviceKind.HomeMaticWired, ChannelAddress, "VAL[UE]\n",
            value);
        var console = new TestConsole().Width(500);

        // Act
        console.Markup(CcuEventLineFormatter.FormatMarkup(record, names));

        // Assert
        console.Output.Should().Be(CcuEventLineFormatter.Format(record, names));
    }

    [Fact]
    public void FormatMarkup_RecordIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CcuEventLineFormatter.FormatMarkup(null!, Names);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FormatMarkup_NamesIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CcuEventLineFormatter.FormatMarkup(CreateRecord(true), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static CcuEventRecord CreateRecord(object? value, CcuDeviceKind kind = CcuDeviceKind.HomeMaticIp)
    {
        return new CcuEventRecord(ReceivedAt, kind, ChannelAddress, "STATE", value);
    }
}
