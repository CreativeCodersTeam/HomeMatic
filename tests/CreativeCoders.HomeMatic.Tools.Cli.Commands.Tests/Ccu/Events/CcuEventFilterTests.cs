using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

public class CcuEventFilterTests
{
    private const string DeviceAddress = "000A1B2C3D4E5F";

    [Theory]
    [InlineData("000A1B2C3D4E5F:1", "STATE")]
    [InlineData("0011223344", "LEVEL")]
    [InlineData("", "")]
    public void Matches_NoFilters_ReturnsTrue(string address, string valueKey)
    {
        // Arrange
        var sut = CcuEventFilter.Create(null, null);

        // Act
        var matches = sut.Matches(address, valueKey);

        // Assert
        matches.Should().BeTrue();
    }

    [Fact]
    public void Create_NoFilters_IsEmpty()
    {
        // Act
        var sut = CcuEventFilter.Create(null, null);

        // Assert
        sut.IsEmpty.Should().BeTrue();
        sut.Addresses.Should().BeEmpty();
        sut.ValueKeys.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmptyLists_IsEmpty()
    {
        // Act
        var sut = CcuEventFilter.Create([], []);

        // Assert
        sut.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Create_OnlyEmptyAndWhiteSpaceEntries_IsEmptyAndMatchesEverything()
    {
        // Act
        var sut = CcuEventFilter.Create(["", "  "], [" ", ""]);

        // Assert
        sut.IsEmpty.Should().BeTrue();
        sut.Matches("000A1B2C3D4E5F:1", "STATE").Should().BeTrue();
    }

    [Theory]
    [InlineData(new[] { DeviceAddress }, null)]
    [InlineData(null, new[] { "STATE" })]
    public void Create_OneFilterSet_IsNotEmpty(string[]? addresses, string[]? valueKeys)
    {
        // Act
        var sut = CcuEventFilter.Create(addresses, valueKeys);

        // Assert
        sut.IsEmpty.Should().BeFalse();
    }

    [Theory]
    [InlineData("000A1B2C3D4E5F:1", true)]
    [InlineData("000A1B2C3D4E5F:12", true)]
    [InlineData("000A1B2C3D4E5F", true)]
    [InlineData("000A1B2C3D4E5F0:1", false)]
    [InlineData("000A1B2C3D4E5F0", false)]
    [InlineData("0011223344:1", false)]
    public void Matches_DeviceAddressFilter_MatchesDeviceAndAllItsChannels(string address, bool expected)
    {
        // Arrange
        var sut = CcuEventFilter.Create([DeviceAddress], null);

        // Act
        var matches = sut.Matches(address, "STATE");

        // Assert
        matches.Should().Be(expected);
    }

    [Theory]
    [InlineData("000A1B2C3D4E5F:1", true)]
    [InlineData("000A1B2C3D4E5F:12", false)]
    [InlineData("000A1B2C3D4E5F:2", false)]
    [InlineData("000A1B2C3D4E5F", false)]
    public void Matches_ChannelAddressFilter_MatchesOnlyThatChannel(string address, bool expected)
    {
        // Arrange
        var sut = CcuEventFilter.Create(["000A1B2C3D4E5F:1"], null);

        // Act
        var matches = sut.Matches(address, "STATE");

        // Assert
        matches.Should().Be(expected);
    }

    [Theory]
    [InlineData("000a1b2c3d4e5f", "000A1B2C3D4E5F:1")]
    [InlineData(" 000a1b2c3d4e5f ", "000A1B2C3D4E5F:1")]
    [InlineData("000A1B2C3D4E5F", "000a1b2c3d4e5f:1")]
    [InlineData("000a1b2c3d4e5f:1", "000A1B2C3D4E5F:1")]
    [InlineData("000A1B2C3D4E5F:1", "000a1b2c3d4e5f:1")]
    public void Matches_AddressInOtherCase_ReturnsTrue(string filterAddress, string eventAddress)
    {
        // Arrange
        var sut = CcuEventFilter.Create([filterAddress], null);

        // Act
        var matches = sut.Matches(eventAddress, "STATE");

        // Assert
        matches.Should().BeTrue();
    }

    [Fact]
    public void Create_Addresses_AreTrimmedAndKeepTheirCase()
    {
        // Act
        var sut = CcuEventFilter.Create([" 000a1b2c3d4e5f ", "0011223344:1 "], null);

        // Assert
        sut.Addresses.Should().Equal("000a1b2c3d4e5f", "0011223344:1");
    }

    [Fact]
    public void Create_ValueKeys_AreTrimmedAndKeepTheirCase()
    {
        // Act
        var sut = CcuEventFilter.Create(null, [" STATE ", "level"]);

        // Assert
        sut.ValueKeys.Should().Equal("STATE", "level");
    }

    [Theory]
    [InlineData("STATE", true)]
    [InlineData("LEVEL", true)]
    [InlineData("state", true)]
    [InlineData("Level", true)]
    [InlineData("WORKING", false)]
    [InlineData("STATE2", false)]
    public void Matches_ValueKeyFilter_MatchesKeyIgnoringCase(string valueKey, bool expected)
    {
        // Arrange
        var sut = CcuEventFilter.Create(null, [" STATE ", "level"]);

        // Act
        var matches = sut.Matches("000A1B2C3D4E5F:1", valueKey);

        // Assert
        matches.Should().Be(expected);
    }

    [Fact]
    public void Create_EmptyEntries_AreIgnored()
    {
        // Act
        var sut = CcuEventFilter.Create(["A", "", "B"], ["STATE", "", " "]);

        // Assert
        sut.Addresses.Should().Equal("A", "B");
        sut.ValueKeys.Should().Equal("STATE");
    }

    [Fact]
    public void Matches_EmptyEntryBetweenAddresses_DoesNotMatchEveryAddress()
    {
        // Arrange
        var sut = CcuEventFilter.Create(["A", "", "B"], null);

        // Act
        var matches = sut.Matches("C:1", "STATE");

        // Assert
        matches.Should().BeFalse();
    }

    [Theory]
    [InlineData("000A1B2C3D4E5F:1", "STATE", true)]
    [InlineData("000A1B2C3D4E5F:1", "LEVEL", false)]
    [InlineData("0011223344:1", "STATE", false)]
    [InlineData("0011223344:1", "LEVEL", false)]
    public void Matches_AddressAndValueKeyFilter_RequiresBoth(string address, string valueKey, bool expected)
    {
        // Arrange
        var sut = CcuEventFilter.Create([DeviceAddress], ["STATE"]);

        // Act
        var matches = sut.Matches(address, valueKey);

        // Assert
        matches.Should().Be(expected);
    }

    [Theory]
    [InlineData("000A1B2C3D4E5F:1", true)]
    [InlineData("0011223344:2", true)]
    [InlineData("0011223344:3", false)]
    [InlineData("0099887766:1", false)]
    public void Matches_SeveralAddresses_MatchesAnyOfThem(string address, bool expected)
    {
        // Arrange
        var sut = CcuEventFilter.Create([DeviceAddress, "0011223344:2"], null);

        // Act
        var matches = sut.Matches(address, "STATE");

        // Assert
        matches.Should().Be(expected);
    }

    [Fact]
    public void Create_NullEntries_AreIgnored()
    {
        // Act
        var sut = CcuEventFilter.Create([null!, "A"], [null!]);

        // Assert
        sut.Addresses.Should().Equal("A");
        sut.ValueKeys.Should().BeEmpty();
    }

    [Theory]
    [InlineData("000A1B2C3D4E5F:", true)]
    [InlineData("000A1B2C3D4E5F:1", false)]
    [InlineData("000A1B2C3D4E5F", false)]
    public void Matches_FilterEntryEndingWithSeparator_MatchesOnlyExactly(string address, bool expected)
    {
        // Arrange
        var sut = CcuEventFilter.Create(["000A1B2C3D4E5F:"], null);

        // Act
        var matches = sut.Matches(address, "STATE");

        // Assert
        matches.Should().Be(expected);
    }

    [Fact]
    public void Matches_EmptyEventAddressWithAddressFilter_ReturnsFalse()
    {
        // Arrange
        var sut = CcuEventFilter.Create([DeviceAddress], null);

        // Act
        var matches = sut.Matches(string.Empty, "STATE");

        // Assert
        matches.Should().BeFalse();
    }

    [Fact]
    public void Matches_NullAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CcuEventFilter.Create(null, null);

        // Act
        var act = () => sut.Matches(null!, "STATE");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Matches_NullValueKey_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CcuEventFilter.Create(null, null);

        // Act
        var act = () => sut.Matches("000A1B2C3D4E5F:1", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
