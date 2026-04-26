using AwesomeAssertions;
using CreativeCoders.HomeMatic.FirmwareBackup;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

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
    public void AddHomeMaticFirmwareBackup_RegistersAcceptAnyCertificateHttpClient()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        services.AddHomeMaticFirmwareBackup();
        var sp = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient(
            FirmwareBackupClientFactory.HttpClientNameAcceptAnyCertificate);
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeMaticFirmwareBackup_AcceptAnyCertificateClient_ConfiguresBypassCallback()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddHomeMaticFirmwareBackup();
        var sp = services.BuildServiceProvider();

        // Act
        var primaryHandler = BuildPrimaryHandler(
            sp,
            FirmwareBackupClientFactory.HttpClientNameAcceptAnyCertificate);

        // Assert
        primaryHandler.Should().BeOfType<HttpClientHandler>();
        ((HttpClientHandler)primaryHandler).ServerCertificateCustomValidationCallback
            .Should().NotBeNull();
    }

    [Fact]
    public void AddHomeMaticFirmwareBackup_DefaultClient_DoesNotBypassCertificate()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddHomeMaticFirmwareBackup();
        var sp = services.BuildServiceProvider();

        // Act
        var primaryHandler = BuildPrimaryHandler(
            sp,
            FirmwareBackupClientFactory.HttpClientName);

        // Assert
        if (primaryHandler is HttpClientHandler httpClientHandler)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback.Should().BeNull();
        }
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

    private static HttpMessageHandler BuildPrimaryHandler(IServiceProvider sp, string clientName)
    {
        var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();
        var clientOptions = optionsMonitor.Get(clientName);

        var builder = A.Fake<HttpMessageHandlerBuilder>();
        var primaryHandler = (HttpMessageHandler)new HttpClientHandler();
        A.CallTo(() => builder.PrimaryHandler).Returns(primaryHandler);
        A.CallToSet(() => builder.PrimaryHandler).Invokes((HttpMessageHandler h) => primaryHandler = h);
        A.CallTo(() => builder.Name).Returns(clientName);

        foreach (var action in clientOptions.HttpMessageHandlerBuilderActions)
        {
            action(builder);
        }

        return primaryHandler;
    }
}
