using CreativeCoders.HomeMatic.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tests;

public class HomeMaticServiceCollectionExtensionsTests
{
    [Fact]
    public void GetRequiredService_CcuClientSupportAdded_GetCcuClientFactory()
    {
        // Arrange
        var services = new ServiceCollection() as IServiceCollection;
        services.AddHomeMatic();

        var sp = services.BuildServiceProvider();

        // Act
        var ccuClientFactory = sp.GetRequiredService<ICcuClientFactory>();

        // Assert
        ccuClientFactory
            .Should()
            .NotBeNull();
    }
}
