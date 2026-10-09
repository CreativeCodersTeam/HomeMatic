using System.IO.Abstractions;
using System.Text.Json;
using AwesomeAssertions;
using CreativeCoders.Core.IO;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Connections;

[Collection(nameof(StaticFileSystemCollection))]
public sealed class CcuConnectionsStoreTests : IDisposable
{
    private const string ConnectionName = "test-ccu";

    private static readonly Uri CcuUrl = new("http://ccu.example.local/");

    private readonly IFileSystemEx _fileSystem = A.Fake<IFileSystemEx>();

    private readonly string _connectionsFile = Path.GetTempFileName();

    private readonly List<string?> _writtenJson = [];

    public CcuConnectionsStoreTests()
    {
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(true);
        A.CallTo(() => _fileSystem.File.OpenRead(A<string>._))
            .ReturnsLazily(() => new FileSystem().File.OpenRead(_connectionsFile));
        A.CallTo(() => _fileSystem.File.WriteAllTextAsync(A<string>._, A<string?>._, A<CancellationToken>._))
            .Invokes((string _, string? json, CancellationToken _) => _writtenJson.Add(json))
            .Returns(Task.CompletedTask);

        FileSys.InstallFileSystemSupport(_fileSystem);
    }

    public void Dispose()
    {
        FileSys.InstallFileSystemSupport(new FileSystemEx());
        File.Delete(_connectionsFile);
    }

    [Theory]
    [InlineData("test-ccu")]
    [InlineData("TEST-CCU")]
    [InlineData("Test-Ccu")]
    public async Task FindConnectionAsync_NameInAnyCase_ReturnsConnection(string name)
    {
        // Arrange
        StoreConnections(new CcuConnectionInfo(CcuUrl, ConnectionName));
        var sut = CreateSut();

        // Act
        var connection = await sut.FindConnectionAsync(name);

        // Assert
        connection.Should().NotBeNull();
        connection.Name.Should().Be(ConnectionName);
        connection.Url.Should().Be(CcuUrl);
    }

    [Fact]
    public async Task FindConnectionAsync_UnknownName_ReturnsNull()
    {
        // Arrange
        StoreConnections(new CcuConnectionInfo(CcuUrl, ConnectionName));
        var sut = CreateSut();

        // Act
        var connection = await sut.FindConnectionAsync("other-ccu");

        // Assert
        connection.Should().BeNull();
    }

    [Fact]
    public async Task FindConnectionAsync_NoConnectionsFile_ReturnsNull()
    {
        // Arrange
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(false);
        var sut = CreateSut();

        // Act
        var connection = await sut.FindConnectionAsync(ConnectionName);

        // Assert
        connection.Should().BeNull();
    }

    [Fact]
    public async Task FindConnectionAsync_SeveralNamesDifferOnlyInCase_ReturnsFirstConnection()
    {
        // Arrange
        StoreConnections(
            new CcuConnectionInfo(CcuUrl, "Test-Ccu"),
            new CcuConnectionInfo(new Uri("http://other.example.local/"), "TEST-CCU"));
        var sut = CreateSut();

        // Act
        var connection = await sut.FindConnectionAsync(ConnectionName);

        // Assert
        connection.Should().NotBeNull();
        connection.Url.Should().Be(CcuUrl);
    }

    [Fact]
    public async Task FindConnectionAsync_NullName_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.FindConnectionAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RemoveConnectionAsync_NameInOtherCase_RemovesConnectionAndReturnsTrue()
    {
        // Arrange
        var otherConnection = new CcuConnectionInfo(new Uri("http://other.example.local/"), "other-ccu");
        StoreConnections(new CcuConnectionInfo(CcuUrl, ConnectionName), otherConnection);
        var sut = CreateSut();

        // Act
        var removed = await sut.RemoveConnectionAsync(ConnectionName.ToUpperInvariant());

        // Assert
        removed.Should().BeTrue();
        var savedConnections = ReadSingleWrite();
        savedConnections.Select(x => x.Name).Should().Equal("other-ccu");
    }

    [Fact]
    public async Task RemoveConnectionAsync_SeveralNamesDifferOnlyInCase_RemovesAllOfThem()
    {
        // Arrange
        StoreConnections(
            new CcuConnectionInfo(CcuUrl, "Test-Ccu"),
            new CcuConnectionInfo(new Uri("http://other.example.local/"), "TEST-CCU"),
            new CcuConnectionInfo(new Uri("http://third.example.local/"), "third-ccu"));
        var sut = CreateSut();

        // Act
        var removed = await sut.RemoveConnectionAsync(ConnectionName);

        // Assert
        removed.Should().BeTrue();
        ReadSingleWrite().Select(x => x.Name).Should().Equal("third-ccu");
    }

    [Fact]
    public async Task RemoveConnectionAsync_UnknownName_ReturnsFalseAndWritesNothing()
    {
        // Arrange
        StoreConnections(new CcuConnectionInfo(CcuUrl, ConnectionName));
        var sut = CreateSut();

        // Act
        var removed = await sut.RemoveConnectionAsync("other-ccu");

        // Assert
        removed.Should().BeFalse();
        _writtenJson.Should().BeEmpty();
    }

    [Fact]
    public async Task AddConnectionAsync_NameDiffersOnlyInCase_ReturnsFalseAndWritesNothing()
    {
        // Arrange
        StoreConnections(new CcuConnectionInfo(CcuUrl, ConnectionName));
        var sut = CreateSut();

        // Act
        var added = await sut.AddConnectionAsync(
            new CcuConnectionInfo(new Uri("http://other.example.local/"), ConnectionName.ToUpperInvariant()));

        // Assert
        added.Should().BeFalse();
        _writtenJson.Should().BeEmpty();
    }

    [Fact]
    public async Task AddConnectionAsync_NewNameAndUrl_AppendsConnectionAndReturnsTrue()
    {
        // Arrange
        StoreConnections(new CcuConnectionInfo(CcuUrl, ConnectionName));
        var sut = CreateSut();

        // Act
        var added = await sut.AddConnectionAsync(
            new CcuConnectionInfo(new Uri("http://other.example.local/"), "other-ccu"));

        // Assert
        added.Should().BeTrue();
        var savedConnections = ReadSingleWrite();
        savedConnections.Select(x => x.Name).Should().Equal(ConnectionName, "other-ccu");
    }

    private static CcuConnectionsStore CreateSut()
    {
        return new CcuConnectionsStore(A.Fake<IAnsiConsole>());
    }

    private void StoreConnections(params CcuConnectionInfo[] connections)
    {
        File.WriteAllText(_connectionsFile, JsonSerializer.Serialize(connections));
    }

    private CcuConnectionInfo[] ReadSingleWrite()
    {
        var json = _writtenJson.Should().ContainSingle().Subject;
        json.Should().NotBeNull();

        return JsonSerializer.Deserialize<CcuConnectionInfo[]>(json) ?? [];
    }
}

[CollectionDefinition(nameof(StaticFileSystemCollection), DisableParallelization = true)]
public sealed class StaticFileSystemCollection;
