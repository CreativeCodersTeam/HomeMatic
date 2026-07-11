using CreativeCoders.HomeMatic.Core;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CompleteCcuDeviceBuildOptionsTests
{
    [Fact]
    public void IsParamSetAllowed_NoWhitelistConfigured_ReturnsTrue()
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions();

        // Act
        var result = options.IsParamSetAllowed("SERVICE");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamSetAllowed_EmptyWhitelist_ReturnsTrue()
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions
        {
            ParamSetWhitelist = []
        };

        // Act
        var result = options.IsParamSetAllowed("SERVICE");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamSetAllowed_WhitelistEntryInDifferentCase_ReturnsTrue()
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions
        {
            ParamSetWhitelist = ["master"]
        };

        // Act
        var result = options.IsParamSetAllowed("MASTER");

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("MASTER", true)]
    [InlineData("master", true)]
    [InlineData("Values", true)]
    [InlineData("SERVICE", false)]
    public void IsParamSetAllowed_WhitelistConfigured_ReturnsExpected(string paramSetKey, bool expected)
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions
        {
            ParamSetWhitelist = ["MASTER", "VALUES"]
        };

        // Act
        var result = options.IsParamSetAllowed(paramSetKey);

        // Assert
        result.Should().Be(expected);
    }
}
