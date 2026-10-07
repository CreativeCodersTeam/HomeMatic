using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Events;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Events;

public class CallbackEndpointResolverTests
{
    private static readonly Uri LoopbackCcuUrl = new("http://127.0.0.1/");

    [Fact]
    public void Resolve_ExplicitHostAndPort_UsesThemAsGiven()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var endpoint = sut.Resolve(new Uri("http://ccu.example.local/"), "192.168.1.20", 53817);

        // Assert
        endpoint.Host.Should().Be("192.168.1.20");
        endpoint.Port.Should().Be(53817);
    }

    [Fact]
    public void Resolve_PortZero_ReturnsFreePortThatCanBeBound()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var endpoint = sut.Resolve(LoopbackCcuUrl, "127.0.0.1", 0);

        // Assert
        endpoint.Port.Should().BeInRange(1, 65535);

        var listener = new TcpListener(IPAddress.Any, endpoint.Port);
        var bind = () => listener.Start();
        bind.Should().NotThrow();
        listener.Stop();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Resolve_NoCallbackHost_UsesLocalAddressRoutedToCcu(string? callbackHost)
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var endpoint = sut.Resolve(LoopbackCcuUrl, callbackHost, 53817);

        // Assert
        endpoint.Host.Should().Be("127.0.0.1");
    }

    [Fact]
    public void Resolve_NoCallbackHostAndCcuHostName_UsesLocalIPv4AddressRoutedToCcu()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var endpoint = sut.Resolve(new Uri("http://localhost/"), null, 53817);

        // Assert
        endpoint.Host.Should().Be("127.0.0.1");
    }

    [Fact]
    public void Resolve_NoCallbackHostAndIPv6OnlyCcu_ThrowsSocketException()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var act = () => sut.Resolve(new Uri("http://[::1]/"), null, 53817);

        // Assert
        act.Should().Throw<SocketException>();
    }

    [Fact]
    public void Resolve_NoCallbackHostAndUnresolvableCcuHost_ThrowsSocketException()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var act = () => sut.Resolve(new Uri("http://ccu.does-not-exist.invalid/"), null, 53817);

        // Assert
        act.Should().Throw<SocketException>();
    }

    [Fact]
    public void Resolve_ExplicitHostAndIPv6OnlyCcu_UsesExplicitHost()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var endpoint = sut.Resolve(new Uri("http://[::1]/"), "192.168.1.20", 53817);

        // Assert
        endpoint.Host.Should().Be("192.168.1.20");
    }

    [Fact]
    public void Resolve_ExplicitHostAndPort_BuildsListenPrefixAndCallbackUrl()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var endpoint = sut.Resolve(LoopbackCcuUrl, "192.168.1.20", 53817);

        // Assert
        endpoint.ListenPrefix.Should().Be("http://+:53817/");
        endpoint.CallbackUrl.Should().Be("http://192.168.1.20:53817/");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Resolve_PortOutOfRange_ThrowsArgumentOutOfRangeException(int callbackPort)
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var act = () => sut.Resolve(LoopbackCcuUrl, "127.0.0.1", callbackPort);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(65535)]
    public void Resolve_PortAtBoundary_UsesPort(int callbackPort)
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var endpoint = sut.Resolve(LoopbackCcuUrl, "127.0.0.1", callbackPort);

        // Assert
        endpoint.Port.Should().Be(callbackPort);
    }

    [Fact]
    public void Resolve_NullCcuUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new CallbackEndpointResolver();

        // Act
        var act = () => sut.Resolve(null!, "127.0.0.1", 53817);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
