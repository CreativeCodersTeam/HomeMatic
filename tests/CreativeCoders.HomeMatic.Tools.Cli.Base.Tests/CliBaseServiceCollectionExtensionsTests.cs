using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests;

public class CliBaseServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHomeMaticCliBase_Called_RegistersCallbackEndpointResolverAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHomeMaticCliBase();

        // Assert
        services.Should().ContainSingle(x => x.ServiceType == typeof(ICallbackEndpointResolver))
            .Which.Should().Match<ServiceDescriptor>(x =>
                x.ImplementationType == typeof(CallbackEndpointResolver) &&
                x.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddHomeMaticCliBase_Called_RegistersConsoleCancelKeySourceAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHomeMaticCliBase();

        // Assert
        services.Should().ContainSingle(x => x.ServiceType == typeof(IConsoleCancelKeySource))
            .Which.Should().Match<ServiceDescriptor>(x =>
                x.ImplementationType == typeof(ConsoleCancelKeySource) &&
                x.Lifetime == ServiceLifetime.Singleton);
    }
}
