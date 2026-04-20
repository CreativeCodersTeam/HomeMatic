using CreativeCoders.HomeMatic.Exporting;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests.Exporting;

public class DeviceExportOptionsTests
{
    [Fact]
    public void IsParamSetAllowed_WithNullWhitelist_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = null };

        // Act
        var result = options.IsParamSetAllowed("MASTER");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamSetAllowed_WithEmptyWhitelist_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = [] };

        // Act
        var result = options.IsParamSetAllowed("MASTER");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamSetAllowed_WithMatchingKey_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = ["MASTER", "VALUES"] };

        // Act
        var result = options.IsParamSetAllowed("MASTER");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamSetAllowed_WithNonMatchingKey_ReturnsFalse()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = ["MASTER", "VALUES"] };

        // Act
        var result = options.IsParamSetAllowed("LINK");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("master")]
    [InlineData("Master")]
    [InlineData("MASTER")]
    public void IsParamSetAllowed_WithDifferentCasing_ReturnsTrue(string key)
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = ["MASTER"] };

        // Act
        var result = options.IsParamSetAllowed(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamSetAllowed_WithEmptyStringKey_ReturnsFalseWhenNotInWhitelist()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = ["MASTER"] };

        // Act
        var result = options.IsParamSetAllowed(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsParamSetAllowed_WithEmptyStringKeyAndEmptyWhitelist_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamSetWhitelist = [] };

        // Act
        var result = options.IsParamSetAllowed(string.Empty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamValueNameAllowed_WithNullWhitelist_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = null };

        // Act
        var result = options.IsParamValueNameAllowed("BOOST_TIME");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamValueNameAllowed_WithEmptyWhitelist_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = [] };

        // Act
        var result = options.IsParamValueNameAllowed("BOOST_TIME");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamValueNameAllowed_WithMatchingName_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["BOOST_TIME", "SET_TEMPERATURE"] };

        // Act
        var result = options.IsParamValueNameAllowed("BOOST_TIME");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamValueNameAllowed_WithNonMatchingName_ReturnsFalse()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["BOOST_TIME", "SET_TEMPERATURE"] };

        // Act
        var result = options.IsParamValueNameAllowed("ACTUAL_TEMPERATURE");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("boost_time")]
    [InlineData("Boost_Time")]
    [InlineData("BOOST_TIME")]
    public void IsParamValueNameAllowed_WithDifferentCasing_ReturnsTrue(string name)
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["BOOST_TIME"] };

        // Act
        var result = options.IsParamValueNameAllowed(name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsParamValueNameAllowed_WithEmptyStringName_ReturnsFalseWhenNotInWhitelist()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["BOOST_TIME"] };

        // Act
        var result = options.IsParamValueNameAllowed(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsParamValueNameAllowed_WithEmptyStringNameAndEmptyWhitelist_ReturnsTrue()
    {
        // Arrange
        var options = new DeviceExportOptions { ParamValueNameWhitelist = [] };

        // Act
        var result = options.IsParamValueNameAllowed(string.Empty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void WriteIndented_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var options = new DeviceExportOptions();

        // Assert
        options.WriteIndented.Should().BeTrue();
    }
}
