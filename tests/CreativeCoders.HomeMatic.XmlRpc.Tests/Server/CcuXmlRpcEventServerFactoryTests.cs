using AwesomeAssertions;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using Microsoft.Extensions.Logging.Abstractions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Server;

public class CcuXmlRpcEventServerFactoryTests
{
    [Fact]
    public void Constructor_NullLoggerFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new CcuXmlRpcEventServerFactory(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespaceListenUrl_ThrowsArgumentException(string? listenUrl)
    {
        // Arrange
        var sut = new CcuXmlRpcEventServerFactory(NullLoggerFactory.Instance);

        // Act
        Action act = () => sut.Create(listenUrl!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task Create_ListenUrl_ReturnsServerWithServerUrlSet()
    {
        // Arrange
        const string listenUrl = "http://+:53817/";
        var sut = new CcuXmlRpcEventServerFactory(NullLoggerFactory.Instance);

        // Act
        await using var server = sut.Create(listenUrl);

        // Assert
        server.ServerUrl.Should().Be(listenUrl);
    }
}
