using AwesomeAssertions;
using CreativeCoders.HomeMatic.Exporting;

namespace CreativeCoders.HomeMatic.Tests.Exporting;

public class DeviceExportOptionsTests
{
    public static TheoryData<string[]?, string, bool> ParamSetAllowedCases => new()
    {
        { null, "MASTER", true },
        { [], "MASTER", true },
        { null, string.Empty, true },
        { [], string.Empty, true },
        { ["MASTER", "VALUES"], "MASTER", true },
        { ["MASTER", "VALUES"], "VALUES", true },
        { ["MASTER", "VALUES"], "LINK", false },
        { ["MASTER"], string.Empty, false },
        { ["MASTER"], "master", true },
        { ["MASTER"], "Master", true },
        { ["master"], "MASTER", true }
    };

    [Theory]
    [MemberData(nameof(ParamSetAllowedCases))]
    public void IsParamSetAllowed_WithWhitelistAndKey_ReturnsExpected(string[]? whitelist, string key, bool expected)
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = whitelist };

        // Act
        var result = options.IsParamSetAllowed(key);

        // Assert
        result.Should().Be(expected);
    }

    public static TheoryData<string[]?, string, bool> ParamValueNameAllowedCases => new()
    {
        { null, "BOOST_TIME", true },
        { [], "BOOST_TIME", true },
        { null, string.Empty, true },
        { [], string.Empty, true },
        { ["BOOST_TIME", "SET_TEMPERATURE"], "BOOST_TIME", true },
        { ["BOOST_TIME", "SET_TEMPERATURE"], "SET_TEMPERATURE", true },
        { ["BOOST_TIME", "SET_TEMPERATURE"], "ACTUAL_TEMPERATURE", false },
        { ["BOOST_TIME"], string.Empty, false },
        { ["BOOST_TIME"], "boost_time", true },
        { ["BOOST_TIME"], "Boost_Time", true },
        { ["boost_time"], "BOOST_TIME", true }
    };

    [Theory]
    [MemberData(nameof(ParamValueNameAllowedCases))]
    public void IsParamValueNameAllowed_WithWhitelistAndName_ReturnsExpected(string[]? whitelist, string name, bool expected)
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = whitelist };

        // Act
        var result = options.IsParamValueNameAllowed(name);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void WriteIndented_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var options = new DeviceExportOptions();

        // Assert
        options.WriteIndented.Should().BeTrue();
    }

    [Fact]
    public void ParamSetWhitelist_DefaultValue_IsNull()
    {
        // Arrange & Act
        var options = new DeviceExportOptions();

        // Assert
        options.ParamSetWhitelist.Should().BeNull();
    }

    [Fact]
    public void ParamValueNameWhitelist_DefaultValue_IsNull()
    {
        // Arrange & Act
        var options = new DeviceExportOptions();

        // Assert
        options.ParamValueNameWhitelist.Should().BeNull();
    }

    [Fact]
    public void SkipServiceParamSet_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var options = new DeviceExportOptions();

        // Assert
        options.SkipServiceParamSet.Should().BeFalse();
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
        var options = new DeviceExportOptions { SkipServiceParamSet = true };

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
        var options = new DeviceExportOptions
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
        var options = new DeviceExportOptions { SkipServiceParamSet = false };

        // Act
        var result = options.IsParamSetAllowed("SERVICE");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ToBuildOptions_SkipServiceParamSetIsSet_IsForwardedToBuildOptions()
    {
        // Arrange
        var options = new DeviceExportOptions { SkipServiceParamSet = true };

        // Act
        var buildOptions = options.ToBuildOptions();

        // Assert
        buildOptions.SkipServiceParamSet.Should().BeTrue();
    }

    [Fact]
    public void ToBuildOptions_SkipServiceParamSetIsNotSet_IsNotForwardedToBuildOptions()
    {
        // Arrange
        var options = new DeviceExportOptions();

        // Act
        var buildOptions = options.ToBuildOptions();

        // Assert
        buildOptions.SkipServiceParamSet.Should().BeFalse();
    }
}
