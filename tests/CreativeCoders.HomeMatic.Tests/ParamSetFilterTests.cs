using AwesomeAssertions;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Tests;

public class ParamSetFilterTests
{
    [Theory]
    [InlineData("SERVICE", true)]
    [InlineData("MASTER", true)]
    [InlineData("VALUES", true)]
    public void IsParamSetAllowed_NoWhitelistAndNoSkip_ReturnsTrue(string paramSetKey, bool expected)
    {
        // Act
        var result = ParamSetFilter.IsParamSetAllowed(null, false, paramSetKey);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("SERVICE", false)]
    [InlineData("service", false)]
    [InlineData("Service", false)]
    [InlineData("MASTER", true)]
    [InlineData("VALUES", true)]
    public void IsParamSetAllowed_SkipServiceParamSetWithoutWhitelist_ReturnsExpected(string paramSetKey,
        bool expected)
    {
        // Act
        var result = ParamSetFilter.IsParamSetAllowed(null, true, paramSetKey);

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
        // Act
        var result = ParamSetFilter.IsParamSetAllowed(["SERVICE", "MASTER"], true, paramSetKey);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsParamSetAllowed_WhitelistContainsServiceAndNoSkip_ServiceIsAllowed()
    {
        // Act
        var result = ParamSetFilter.IsParamSetAllowed(["SERVICE"], false, "SERVICE");

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("MASTER", true)]
    [InlineData("master", true)]
    [InlineData("VALUES", false)]
    public void IsParamSetAllowed_WhitelistWithoutSkip_DelegatesToWhitelistFilter(string paramSetKey, bool expected)
    {
        // Act
        var result = ParamSetFilter.IsParamSetAllowed(["MASTER"], false, paramSetKey);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsParamSetAllowed_ParamSetKeyIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ParamSetFilter.IsParamSetAllowed(null, false, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("paramSetKey");
    }

    [Fact]
    public void IsParamSetAllowed_EmptyWhitelist_AllowsEverythingExceptSkippedService()
    {
        // Act
        var serviceAllowed = ParamSetFilter.IsParamSetAllowed([], true, "SERVICE");
        var masterAllowed = ParamSetFilter.IsParamSetAllowed([], true, "MASTER");

        // Assert
        serviceAllowed.Should().BeFalse();
        masterAllowed.Should().BeTrue();
    }
}
