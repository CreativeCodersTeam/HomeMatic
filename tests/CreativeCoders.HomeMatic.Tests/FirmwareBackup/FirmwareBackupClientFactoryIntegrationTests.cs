using System.IO.Abstractions;
using System.Net;
using System.Text;
using AwesomeAssertions;
using CreativeCoders.Core.IO;
using CreativeCoders.HomeMatic.FirmwareBackup;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

public class FirmwareBackupClientFactoryIntegrationTests
{
    private const string FakeSessionId = "session-id-xyz";
    private static readonly byte[] BackupPayload = Encoding.UTF8.GetBytes("BACKUP-CONTENT");

    [Fact]
    public async Task CreateBackupAsync_HappyPath_PerformsLoginDownloadAndLogout()
    {
        // Arrange
        var (factory, handler) = CreateFactory();
        handler.EnqueueJsonResponse($"{{\"version\":\"1.1\",\"result\":\"{FakeSessionId}\",\"error\":null}}");
        handler.EnqueueBinaryResponse(BackupPayload, "ccu_backup.sbk");
        handler.EnqueueJsonResponse("{\"version\":\"1.1\",\"result\":true,\"error\":null}");

        var options = new FirmwareBackupOptions(
            new Uri("https://ccu.example.local"),
            new NetworkCredential("Admin", "secret"));

        var client = factory.Create(options);

        // Act
        await using var result = await client.CreateBackupAsync();
        using var ms = new MemoryStream();
        await result.Content.CopyToAsync(ms);

        // Assert
        ms.ToArray().Should().BeEquivalentTo(BackupPayload);
        result.FileName.Should().Be("ccu_backup.sbk");

        handler.Requests.Should().HaveCount(2);
        handler.Requests[0].Uri.AbsolutePath.Should().Be("/api/homematic.cgi");
        handler.Requests[0].Body.Should().Contain("\"Session.login\"").And.Contain("\"Admin\"");
        handler.Requests[1].Uri.AbsolutePath.Should().Be("/config/cp_security.cgi");
        handler.Requests[1].Method.Should().Be(HttpMethod.Get);
        handler.Requests[1].Uri.Query.Should().Contain($"sid=%40{FakeSessionId}%40").And.Contain("action=create_backup");
    }

    [Fact]
    public async Task CreateBackupAsync_HappyPath_LogoutCalledOnDispose()
    {
        // Arrange
        var (factory, handler) = CreateFactory();
        handler.EnqueueJsonResponse($"{{\"result\":\"{FakeSessionId}\",\"error\":null}}");
        handler.EnqueueBinaryResponse(BackupPayload, "backup.sbk");
        handler.EnqueueJsonResponse("{\"result\":true,\"error\":null}");

        var client = factory.Create(new FirmwareBackupOptions(
            new Uri("https://ccu.example.local"),
            new NetworkCredential("Admin", "x")));

        // Act
        var result = await client.CreateBackupAsync();
        await result.DisposeAsync();

        // Assert
        handler.Requests.Should().HaveCount(3);
        handler.Requests[2].Body.Should().Contain("\"Session.logout\"").And.Contain(FakeSessionId);
    }

    [Fact]
    public async Task CreateBackupAsync_DownloadFails_LogoutStillCalled()
    {
        // Arrange
        var (factory, handler) = CreateFactory();
        handler.EnqueueJsonResponse($"{{\"result\":\"{FakeSessionId}\",\"error\":null}}");
        handler.EnqueueResponse(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("boom")
        });
        handler.EnqueueJsonResponse("{\"result\":true,\"error\":null}");

        var client = factory.Create(new FirmwareBackupOptions(
            new Uri("https://ccu.example.local"),
            new NetworkCredential("Admin", "x")));

        // Act
        var act = async () => await client.CreateBackupAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<FirmwareBackupException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        handler.Requests.Should().HaveCount(3);
        handler.Requests[2].Body.Should().Contain("\"Session.logout\"");
    }

    [Fact]
    public async Task CreateBackupAsync_LoginFailsWithError_ThrowsFirmwareBackupException()
    {
        // Arrange
        var (factory, handler) = CreateFactory();
        handler.EnqueueJsonResponse(
            "{\"result\":null,\"error\":{\"code\":2,\"message\":\"invalid credentials\"}}");

        var client = factory.Create(new FirmwareBackupOptions(
            new Uri("https://ccu.example.local"),
            new NetworkCredential("Admin", "wrong")));

        // Act
        var act = async () => await client.CreateBackupAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<FirmwareBackupException>();
        ex.Which.Message.Should().Contain("invalid credentials");
    }

    [Fact]
    public async Task CreateBackupToFileAsync_WritesContentToFile()
    {
        // Arrange
        var (factory, handler) = CreateFactory();
        handler.EnqueueJsonResponse($"{{\"result\":\"{FakeSessionId}\",\"error\":null}}");
        handler.EnqueueBinaryResponse(BackupPayload, "ccu_backup.sbk");
        handler.EnqueueJsonResponse("{\"result\":true,\"error\":null}");

        var tempDir = Path.Combine(Path.GetTempPath(), "fwbackup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var client = factory.Create(new FirmwareBackupOptions(
                new Uri("https://ccu.example.local"),
                new NetworkCredential("Admin", "x")));

            // Act
            var path = await client.CreateBackupToFileAsync(tempDir);

            // Assert
            File.Exists(path).Should().BeTrue();
            (await File.ReadAllBytesAsync(path)).Should().BeEquivalentTo(BackupPayload);
            Path.GetFileName(path).Should().Be("ccu_backup.sbk");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task CreateBackupToFileAsync_WithExplicitFilePath_UsesGivenPath()
    {
        // Arrange
        var (factory, handler) = CreateFactory();
        handler.EnqueueJsonResponse($"{{\"result\":\"{FakeSessionId}\",\"error\":null}}");
        handler.EnqueueBinaryResponse(BackupPayload, "default.sbk");
        handler.EnqueueJsonResponse("{\"result\":true,\"error\":null}");

        var tempFile = Path.Combine(Path.GetTempPath(), "fwbackup-" + Guid.NewGuid().ToString("N") + ".sbk");

        try
        {
            var client = factory.Create(new FirmwareBackupOptions(
                new Uri("https://ccu.example.local"),
                new NetworkCredential("Admin", "x")));

            // Act
            var path = await client.CreateBackupToFileAsync(tempFile);

            // Assert
            path.Should().Be(tempFile);
            (await File.ReadAllBytesAsync(path)).Should().BeEquivalentTo(BackupPayload);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private static (IFirmwareBackupClientFactory Factory, QueueingHttpMessageHandler Handler) CreateFactory()
    {
        var handler = new QueueingHttpMessageHandler();
        var httpClientFactory = new SingleHandlerHttpClientFactory(handler);
        var fileSystem = new ServiceCollection()
            .AddFileSystem()
            .BuildServiceProvider()
            .GetRequiredService<IFileSystem>();
        var factory = new FirmwareBackupClientFactory(httpClientFactory, fileSystem);
        return (factory, handler);
    }
}
