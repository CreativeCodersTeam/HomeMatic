using AwesomeAssertions;
using CreativeCoders.HomeMatic.FirmwareBackup;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

public class FirmwareBackupServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHomeMaticFirmwareBackup_RegistersFactory()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.AddHomeMaticFirmwareBackup();
        var sp = services.BuildServiceProvider();

        // Assert
        var factory = sp.GetRequiredService<IFirmwareBackupClientFactory>();
        factory.Should().BeOfType<FirmwareBackupClientFactory>();
    }

    [Fact]
    public void AddHomeMaticFirmwareBackup_RegistersHttpClientFactory()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.AddHomeMaticFirmwareBackup();
        var sp = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient(FirmwareBackupClientFactory.HttpClientName);
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeMatic_RegistersFirmwareBackupFactory()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.AddHomeMatic();
        var sp = services.BuildServiceProvider();

        // Assert
        sp.GetRequiredService<IFirmwareBackupClientFactory>().Should().NotBeNull();
    }
}
