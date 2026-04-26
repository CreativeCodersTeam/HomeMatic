using System.IO.Abstractions;
using System.Net;
using AwesomeAssertions;
using CreativeCoders.Core.IO;
using CreativeCoders.HomeMatic.FirmwareBackup;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

public class FirmwareBackupClientFactoryTests
{
    [Fact]
    public void Create_WithAcceptAnyServerCertificateTrue_UsesAcceptAnyHttpClient()
    {
        // Arrange
        var (factory, httpClientFactory) = CreateSut();
        var options = new FirmwareBackupOptions(
            new Uri("https://ccu.local"),
            new NetworkCredential("user", "pass"))
        {
            AcceptAnyServerCertificate = true
        };

        // Act
        factory.Create(options);

        // Assert
        A.CallTo(() => httpClientFactory.CreateClient(
                FirmwareBackupClientFactory.HttpClientNameAcceptAnyCertificate))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => httpClientFactory.CreateClient(FirmwareBackupClientFactory.HttpClientName))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Create_WithAcceptAnyServerCertificateFalse_UsesDefaultHttpClient()
    {
        // Arrange
        var (factory, httpClientFactory) = CreateSut();
        var options = new FirmwareBackupOptions(
            new Uri("https://ccu.local"),
            new NetworkCredential("user", "pass"))
        {
            AcceptAnyServerCertificate = false
        };

        // Act
        factory.Create(options);

        // Assert
        A.CallTo(() => httpClientFactory.CreateClient(FirmwareBackupClientFactory.HttpClientName))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => httpClientFactory.CreateClient(
                FirmwareBackupClientFactory.HttpClientNameAcceptAnyCertificate))
            .MustNotHaveHappened();
    }

    private static (FirmwareBackupClientFactory Factory, IHttpClientFactory HttpClientFactory) CreateSut()
    {
        var httpClientFactory = A.Fake<IHttpClientFactory>();
        A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
            .ReturnsLazily(() => new HttpClient(new QueueingHttpMessageHandler(), disposeHandler: false));

        var fileSystem = new ServiceCollection()
            .AddFileSystem()
            .BuildServiceProvider()
            .GetRequiredService<IFileSystem>();

        var factory = new FirmwareBackupClientFactory(httpClientFactory, fileSystem);
        return (factory, httpClientFactory);
    }
}
