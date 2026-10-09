using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;
using CreativeCoders.SysConsole.Cli.Parsing;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

public class MonitorCcuEventsOptionsTests
{
    [Fact]
    public void Parse_ShortFilterOptions_SplitsValuesOnComma()
    {
        // Arrange
        var parser = new OptionParser(typeof(MonitorCcuEventsOptions));

        // Act
        var options = (MonitorCcuEventsOptions)parser.Parse(
            ["home", "-a", "000A1B2C3D4E5F,0011223344:1", "-k", "STATE"]);

        // Assert
        options.Name.Should().Be("home");
        options.Addresses.Should().Equal("000A1B2C3D4E5F", "0011223344:1");
        options.ValueKeys.Should().Equal("STATE");
    }

    [Fact]
    public void Parse_LongFilterOptionsWithEmptyEntry_FilterIgnoresEmptyEntry()
    {
        // Arrange
        var parser = new OptionParser(typeof(MonitorCcuEventsOptions));

        // Act
        var options = (MonitorCcuEventsOptions)parser.Parse(
            ["home", "--address", "A,,B", "--value-key", "state,level"]);
        var filter = CcuEventFilter.Create(options.Addresses, options.ValueKeys);

        // Assert
        filter.Addresses.Should().Equal("A", "B");
        filter.ValueKeys.Should().Equal("state", "level");
    }

    [Fact]
    public void Parse_NoFilterOptions_FilterIsEmpty()
    {
        // Arrange
        var parser = new OptionParser(typeof(MonitorCcuEventsOptions));

        // Act
        var options = (MonitorCcuEventsOptions)parser.Parse(["home"]);
        var filter = CcuEventFilter.Create(options.Addresses, options.ValueKeys);

        // Assert
        filter.IsEmpty.Should().BeTrue();
    }
}
