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

    [Fact]
    public void SkipServiceParamSet_NotConfigured_IsFalse()
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions();

        // Act
        var result = options.SkipServiceParamSet;

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("SERVICE", false)]
    [InlineData("service", false)]
    [InlineData("MASTER", true)]
    [InlineData("VALUES", true)]
    public void IsParamSetAllowed_SkipServiceParamSetWithoutWhitelist_ReturnsExpected(string paramSetKey,
        bool expected)
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions
        {
            SkipServiceParamSet = true
        };

        // Act
        var result = options.IsParamSetAllowed(paramSetKey);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("SERVICE", false)]
    [InlineData("MASTER", true)]
    [InlineData("VALUES", false)]
    public void IsParamSetAllowed_SkipServiceParamSetAndWhitelistContainsService_SkipWins(string paramSetKey,
        bool expected)
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions
        {
            SkipServiceParamSet = true,
            ParamSetWhitelist = ["SERVICE", "MASTER"]
        };

        // Act
        var result = options.IsParamSetAllowed(paramSetKey);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsParamSetAllowed_SkipServiceParamSetIsFalse_ServiceIsAllowed()
    {
        // Arrange
        var options = new CompleteCcuDeviceBuildOptions
        {
            SkipServiceParamSet = false
        };

        // Act
        var result = options.IsParamSetAllowed("SERVICE");

        // Assert
        result.Should().BeTrue();
    }
}
