using CreativeCoders.HomeMatic.Core;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tests;

public class HomeMaticServiceCollectionExtensionsTests
{
    [Fact]
    public void GetRequiredService_CcuClientSupportAdded_GetCcuClientFactory()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
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
