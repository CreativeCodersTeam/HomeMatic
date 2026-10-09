using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests;

public class CcuAddressTests
{
    [Theory]
    [InlineData("000A1B2C3D4E5F:1", "000A1B2C3D4E5F")]
    [InlineData("000A1B2C3D4E5F", "000A1B2C3D4E5F")]
    [InlineData("000A1B2C3D4E5F:", "000A1B2C3D4E5F")]
    [InlineData("000A1B2C3D4E5F:1:2", "000A1B2C3D4E5F")]
    [InlineData(":1", "")]
    [InlineData("", "")]
    public void GetDeviceAddress_Address_ReturnsPartBeforeFirstSeparator(string address, string expected)
    {
        // Act
        var deviceAddress = CcuAddress.GetDeviceAddress(address);

        // Assert
        deviceAddress.Should().Be(expected);
    }

    [Theory]
    [InlineData("000A1B2C3D4E5F:1", "1")]
    [InlineData("000A1B2C3D4E5F:", "")]
    [InlineData("000A1B2C3D4E5F:1:2", "1:2")]
    [InlineData(":1", "1")]
    public void GetChannel_ChannelAddress_ReturnsPartAfterFirstSeparator(string address, string expected)
    {
        // Act
        var channel = CcuAddress.GetChannel(address);

        // Assert
        channel.Should().Be(expected);
    }

    [Theory]
    [InlineData("000A1B2C3D4E5F")]
    [InlineData("")]
    public void GetChannel_DeviceAddress_ReturnsNull(string address)
    {
        // Act
        var channel = CcuAddress.GetChannel(address);

        // Assert
        channel.Should().BeNull();
    }

    [Fact]
    public void GetDeviceAddress_NullAddress_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CcuAddress.GetDeviceAddress(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetChannel_NullAddress_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CcuAddress.GetChannel(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
