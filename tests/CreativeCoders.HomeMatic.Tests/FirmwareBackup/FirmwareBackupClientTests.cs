using System.IO.Abstractions;
using System.Net;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.FirmwareBackup;
using CreativeCoders.HomeMatic.FirmwareBackup.Internal;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

public class FirmwareBackupClientTests
{
    private const string SessionId = "sid-123";

    private static readonly byte[] DownloadedContent = "DOWNLOADED-BACKUP"u8.ToArray();

    [Fact]
    public async Task CreateBackupAsync_HappyPath_VerifiesBackupAndReturnsReadableContent()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await using var result = await sut.Client.CreateBackupAsync();
        using var buffer = new MemoryStream();
        await result.Content.CopyToAsync(buffer);

        // Assert
        A.CallTo(() => sut.Verifier.VerifyAsync(A<Stream>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        buffer.ToArray().Should().BeEquivalentTo(DownloadedContent);
        result.FileName.Should().Be("ccu_backup.sbk");
    }

    [Fact]
    public async Task CreateBackupAsync_PassesDownloadedContentToVerifier()
    {
        // Arrange
        var sut = CreateSut();
        byte[]? verifiedContent = null;
        A.CallTo(() => sut.Verifier.VerifyAsync(A<Stream>._, A<CancellationToken>._))
            .Invokes((Stream content, CancellationToken _) =>
            {
                var position = content.Position;
                using var copy = new MemoryStream();
                content.CopyTo(copy);
                verifiedContent = copy.ToArray();
                content.Position = position;
            });

        // Act
        await using var result = await sut.Client.CreateBackupAsync();

        // Assert
        verifiedContent.Should().BeEquivalentTo(DownloadedContent);
    }

    [Fact]
    public async Task CreateBackupAsync_HappyPath_DisposesHttpResources()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await using var result = await sut.Client.CreateBackupAsync();

        // Assert
        A.CallTo(() => sut.HttpResources.DisposeAsync()).MustHaveHappened();
    }

    [Fact]
    public async Task CreateBackupAsync_DisposingResult_DisposesContentStream()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.Client.CreateBackupAsync();
        var content = result.Content;
        await result.DisposeAsync();

        // Assert
        content.Invoking(s => s.ReadByte()).Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task CreateBackupAsync_VerificationFails_PropagatesException()
    {
        // Arrange
        var sut = CreateSut();
        A.CallTo(() => sut.Verifier.VerifyAsync(A<Stream>._, A<CancellationToken>._))
            .Throws(new InvalidFirmwareBackupException("invalid backup"));

        // Act
        var act = async () => await sut.Client.CreateBackupAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Be("invalid backup");
    }

    [Fact]
    public async Task CreateBackupAsync_VerificationFails_LogsOutSession()
    {
        // Arrange
        var sut = CreateSut();
        A.CallTo(() => sut.Verifier.VerifyAsync(A<Stream>._, A<CancellationToken>._))
            .Throws(new InvalidFirmwareBackupException("invalid backup"));

        // Act
        var act = async () => await sut.Client.CreateBackupAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        A.CallTo(() => sut.SessionClient.LogoutAsync(SessionId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateBackupAsync_VerificationFails_DisposesHttpResources()
    {
        // Arrange
        var sut = CreateSut();
        A.CallTo(() => sut.Verifier.VerifyAsync(A<Stream>._, A<CancellationToken>._))
            .Throws(new InvalidFirmwareBackupException("invalid backup"));

        // Act
        var act = async () => await sut.Client.CreateBackupAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        A.CallTo(() => sut.HttpResources.DisposeAsync()).MustHaveHappened();
    }

    private static SutContext CreateSut()
    {
        var sessionClient = A.Fake<ICcuSessionClient>();
        var downloader = A.Fake<IFirmwareBackupDownloader>();
        var verifier = A.Fake<ICcuBackupVerifier>();
        var fileSystem = A.Fake<IFileSystem>();
        var httpResources = A.Fake<IAsyncDisposable>();

        A.CallTo(() => sessionClient.LoginAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .Returns(SessionId);

        A.CallTo(() => downloader.DownloadAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily(() => new FirmwareBackupDownloadResult(
                new MemoryStream(DownloadedContent),
                "ccu_backup.sbk",
                DownloadedContent.Length,
                httpResources));

        var options = new FirmwareBackupOptions(
            new Uri("https://ccu.example.local"),
            new NetworkCredential("Admin", "secret"));

        var client = new FirmwareBackupClient(sessionClient, downloader, verifier, options, fileSystem);

        return new SutContext(client, sessionClient, downloader, verifier, httpResources);
    }

    private sealed record SutContext(
        FirmwareBackupClient Client,
        ICcuSessionClient SessionClient,
        IFirmwareBackupDownloader Downloader,
        ICcuBackupVerifier Verifier,
        IAsyncDisposable HttpResources);
}
