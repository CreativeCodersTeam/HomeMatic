using AwesomeAssertions;
using CreativeCoders.HomeMatic.FirmwareBackup;
using CreativeCoders.HomeMatic.FirmwareBackup.Internal;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

public class CcuBackupVerifierTests
{
    [Fact]
    public async Task VerifyAsync_ValidBackup_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        using var content = new MemoryStream(CcuBackupTestData.CreateValidBackup());

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_EntriesWithPathPrefix_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("./signature", "sig"u8.ToArray()),
            ("./user_data.tar.gz", CcuBackupTestData.Gzip("data"u8.ToArray())));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_VerifiesRegardlessOfInitialPosition()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        using var content = new MemoryStream(CcuBackupTestData.CreateValidBackup())
        {
            Position = 5
        };

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_MissingSignature_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("user_data.tar.gz", CcuBackupTestData.Gzip("data"u8.ToArray())));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("signature");
    }

    [Fact]
    public async Task VerifyAsync_EmptySignature_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("signature", []),
            ("user_data.tar.gz", CcuBackupTestData.Gzip("data"u8.ToArray())));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_MissingUserData_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("signature", "sig"u8.ToArray()));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("user_data.tar.gz");
    }

    [Fact]
    public async Task VerifyAsync_EmptyUserData_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("signature", "sig"u8.ToArray()),
            ("user_data.tar.gz", []));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_UserDataNotGzipCompressed_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("signature", "sig"u8.ToArray()),
            ("user_data.tar.gz", "this-is-not-gzip"u8.ToArray()));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_ContentIsNotATar_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        using var content = new MemoryStream("not-a-tar-archive-at-all-just-some-random-bytes"u8.ToArray());

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_EmptyContent_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        using var content = new MemoryStream([]);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_NullContent_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();

        // Act
        var act = async () => await sut.VerifyAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task VerifyAsync_InvalidBackup_ThrownExceptionIsAFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        using var content = new MemoryStream("garbage"u8.ToArray());

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<FirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_UserDataSingleByte_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("signature", "sig"u8.ToArray()),
            ("user_data.tar.gz", [0x1F]));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_UserDataPartialGzipMagic_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("signature", "sig"u8.ToArray()),
            ("user_data.tar.gz", [0x1F, 0x00, 0x42]));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_EntriesWithBackslashPrefix_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            (@"dir\signature", "sig"u8.ToArray()),
            (@"dir\user_data.tar.gz", CcuBackupTestData.Gzip("data"u8.ToArray())));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_ValidBackupWithExtraEntries_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
            ("readme.txt", "info"u8.ToArray()),
            ("signature", "sig"u8.ToArray()),
            ("user_data.tar.gz", CcuBackupTestData.Gzip("data"u8.ToArray())),
            ("extra.bin", "x"u8.ToArray()));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_TruncatedTar_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var truncated = CcuBackupTestData.CreateValidBackup()[..600];
        using var content = new MemoryStream(truncated);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_NonSeekableStream_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        await using var content = new ForwardOnlyStream(CcuBackupTestData.CreateValidBackup());

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_UserDataGzipHeaderButCorruptBody_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var corruptGzip = CcuBackupTestData.Gzip("payload-data-to-compress"u8.ToArray());
        corruptGzip[corruptGzip.Length / 2] ^= 0xFF; // corrupt the deflate body -> valid header, CRC mismatch
        var backup = CcuBackupTestData.CreateTar(
            ("signature", "sig"u8.ToArray()),
            ("user_data.tar.gz", corruptGzip));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        using var content = new MemoryStream(CcuBackupTestData.CreateValidBackup());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await sut.VerifyAsync(content, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed class ForwardOnlyStream(byte[] data) : Stream
    {
        private readonly MemoryStream _inner = new(data);

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
