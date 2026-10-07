using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Events;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Events;

public class CallbackEndpointTests
{
    [Theory]
    [InlineData("192.168.1.20", "http://192.168.1.20:5000/")]
    [InlineData("monitor.example.local", "http://monitor.example.local:5000/")]
    public void CallbackUrl_IPv4AddressOrHostName_UsesHostAsGiven(string host, string expectedUrl)
    {
        // Arrange
        var sut = new CallbackEndpoint(host, 5000);

        // Act
        var callbackUrl = sut.CallbackUrl;

        // Assert
        callbackUrl.Should().Be(expectedUrl);
    }

    [Theory]
    [InlineData("fe80::1", "http://[fe80::1]:5000/")]
    [InlineData("::1", "http://[::1]:5000/")]
    [InlineData("2001:db8::20", "http://[2001:db8::20]:5000/")]
    public void CallbackUrl_IPv6Address_EnclosesHostInBrackets(string host, string expectedUrl)
    {
        // Arrange
        var sut = new CallbackEndpoint(host, 5000);

        // Act
        var callbackUrl = sut.CallbackUrl;

        // Assert
        callbackUrl.Should().Be(expectedUrl);
    }

    [Fact]
    public void CallbackUrl_IPv6AddressAlreadyInBrackets_DoesNotAddBrackets()
    {
        // Arrange
        var sut = new CallbackEndpoint("[fe80::1]", 5000);

        // Act
        var callbackUrl = sut.CallbackUrl;

        // Assert
        callbackUrl.Should().Be("http://[fe80::1]:5000/");
    }

    [Fact]
    public void CallbackUrl_IPv6Address_IsParsableUriWithSameHostAndPort()
    {
        // Arrange
        var sut = new CallbackEndpoint("fe80::1", 5000);

        // Act
        var uri = new Uri(sut.CallbackUrl);

        // Assert
        uri.Host.Should().Be("[fe80::1]");
        uri.Port.Should().Be(5000);
    }
}
