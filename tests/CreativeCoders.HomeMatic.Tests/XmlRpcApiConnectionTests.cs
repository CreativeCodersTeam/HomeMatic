using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class XmlRpcApiConnectionTests
{
    [Fact]
    public void Ctor_WithValidArguments_AssignsProperties()
    {
        // Arrange
        var address = new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMatic);
        var api = A.Fake<IHomeMaticXmlRpcApi>();

        // Act
        var connection = new XmlRpcApiConnection(address, api);

        // Assert
        connection.Address.Should().BeSameAs(address);
        connection.Api.Should().BeSameAs(api);
        connection.CcuName.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_WithNullAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();

        // Act
        Action act = () => _ = new XmlRpcApiConnection(null!, api);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_WithNullApi_ThrowsArgumentNullException()
    {
        // Arrange
        var address = new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMatic);

        // Act
        Action act = () => _ = new XmlRpcApiConnection(address, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CcuName_CanBeAssigned()
    {
        // Arrange
        var connection = new XmlRpcApiConnection(
            new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMatic),
            A.Fake<IHomeMaticXmlRpcApi>());

        // Act
        connection.CcuName = "ccu1";

        // Assert
        connection.CcuName.Should().Be("ccu1");
    }
}
