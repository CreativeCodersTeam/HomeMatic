using AwesomeAssertions;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests;

public class XmlRpcServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHomeMaticXmlRpc_WithLogging_ResolvesCcuXmlRpcEventServerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging().AddHomeMaticXmlRpc();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var factory = serviceProvider.GetService<ICcuXmlRpcEventServerFactory>();

        // Assert
        factory.Should().BeOfType<CcuXmlRpcEventServerFactory>();
    }

    [Fact]
    public void AddHomeMaticXmlRpc_CustomFactoryRegisteredBefore_KeepsCustomFactory()
    {
        // Arrange
        var customFactory = A.Fake<ICcuXmlRpcEventServerFactory>();
        var services = new ServiceCollection();
        services.AddSingleton(customFactory);
        services.AddLogging().AddHomeMaticXmlRpc();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var factory = serviceProvider.GetService<ICcuXmlRpcEventServerFactory>();

        // Assert
        factory.Should().BeSameAs(customFactory);
    }
}
