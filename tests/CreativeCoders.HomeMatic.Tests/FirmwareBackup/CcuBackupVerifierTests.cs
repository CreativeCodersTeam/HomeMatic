using System.Text;
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
            CcuBackupTestData.CreateValidEntries()
                .Select(entry => ("./" + entry.Name, entry.Content))
                .ToArray());
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
        var backup = CcuBackupTestData.CreateBackupWithout("signature");
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
        var backup = CcuBackupTestData.CreateBackupWith("signature", []);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_MissingUsrLocal_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWithout("usr_local.tar.gz");
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("usr_local.tar.gz");
    }

    [Fact]
    public async Task VerifyAsync_EmptyUsrLocal_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", []);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_UsrLocalNotGzipCompressed_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", "this-is-not-gzip"u8.ToArray());
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
    public async Task VerifyAsync_UsrLocalSingleByte_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", [0x1F]);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_UsrLocalPartialGzipMagic_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", [0x1F, 0x00, 0x42]);
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
            CcuBackupTestData.CreateValidEntries()
                .Select(entry => (@"dir\" + entry.Name, entry.Content))
                .ToArray());
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
        [
            ("readme.txt", "info"u8.ToArray()),
            .. CcuBackupTestData.CreateValidEntries(),
            ("extra.bin", "x"u8.ToArray())
        ]);
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
    public async Task VerifyAsync_UsrLocalGzipHeaderButCorruptBody_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var corruptGzip = CcuBackupTestData.CreateValidUserData();
        corruptGzip[corruptGzip.Length / 2] ^= 0xFF; // corrupt the deflate body -> valid header, CRC mismatch
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", corruptGzip);
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

    [Theory]
    [InlineData("sig")]
    [InlineData("d37893be27a7d3642fe6ff8b9ea78bc")]
    [InlineData("d37893be27a7d3642fe6ff8b9ea78bc55")]
    [InlineData("zz7893be27a7d3642fe6ff8b9ea78bc5")]
    public async Task VerifyAsync_SignatureWithInvalidFormat_ThrowsInvalidFirmwareBackupException(
        string signature)
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("signature", Encoding.UTF8.GetBytes(signature));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("signature");
    }

    [Fact]
    public async Task VerifyAsync_SignatureWithUppercaseHexCharacters_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith(
            "signature",
            "D37893BE27A7D3642FE6FF8B9EA78BC5\n"u8.ToArray());
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_MissingFirmwareVersion_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWithout("firmware_version");
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("firmware_version");
    }

    [Fact]
    public async Task VerifyAsync_EmptyFirmwareVersion_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("firmware_version", []);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Theory]
    [InlineData("3.83.6")]
    [InlineData("VERSION=")]
    [InlineData("VERSION=abc")]
    [InlineData("VERSION=.")]
    [InlineData("VERSION=....")]
    [InlineData("FIRMWARE=3.83.6")]
    public async Task VerifyAsync_FirmwareVersionWithInvalidFormat_ThrowsInvalidFirmwareBackupException(
        string firmwareVersion)
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith(
            "firmware_version",
            Encoding.UTF8.GetBytes(firmwareVersion));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("firmware_version");
    }

    [Theory]
    [InlineData("VERSION=3.85")]
    [InlineData("VERSION=3.83.6.20250101")]
    public async Task VerifyAsync_FirmwareVersionWithOtherVersionFormats_DoesNotThrow(string firmwareVersion)
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith(
            "firmware_version",
            Encoding.UTF8.GetBytes(firmwareVersion + "\n"));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_MissingKeyIndex_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWithout("key_index");
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("key_index");
    }

    [Fact]
    public async Task VerifyAsync_EmptyKeyIndex_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("key_index", []);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("+5")]
    [InlineData("99999999999")]
    public async Task VerifyAsync_KeyIndexWithInvalidFormat_ThrowsInvalidFirmwareBackupException(string keyIndex)
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("key_index", Encoding.UTF8.GetBytes(keyIndex));
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("key_index");
    }

    [Fact]
    public async Task VerifyAsync_KeyIndexWithPositiveInteger_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("key_index", "5\n"u8.ToArray());
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_UsrLocalWithoutUsrLocalContent_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var userData = CcuBackupTestData.Gzip(CcuBackupTestData.CreateTar(
            ("etc/config/settings.conf", "settings"u8.ToArray())));
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", userData);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("usr_local.tar.gz");
    }

    [Fact]
    public async Task VerifyAsync_UsrLocalGzipButNotATar_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var userData = CcuBackupTestData.Gzip("just-some-text-that-is-not-a-tar-archive"u8.ToArray());
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", userData);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("usr_local.tar.gz");
    }

    [Fact]
    public async Task VerifyAsync_DuplicateSignatureEntryInvalidLast_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
        [
            .. CcuBackupTestData.CreateValidEntries(),
            ("signature", "not-a-valid-signature"u8.ToArray())
        ]);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Fact]
    public async Task VerifyAsync_DuplicateSignatureEntryValidLast_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
        [
            ("signature", "not-a-valid-signature"u8.ToArray()),
            .. CcuBackupTestData.CreateValidEntries()
        ]);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_CalledTwiceOnSameStream_DoesNotThrow()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        using var content = new MemoryStream(CcuBackupTestData.CreateValidBackup());
        await sut.VerifyAsync(content);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_WhitespaceOnlySignature_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateBackupWith("signature", " \n"u8.ToArray());
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
    }

    [Theory]
    [InlineData("./usr/local/etc/config/ids")]
    [InlineData("/usr/local/etc/config/ids")]
    [InlineData("usr/local")]
    public async Task VerifyAsync_UsrLocalContentWithNameVariants_DoesNotThrow(string innerEntryName)
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var userData = CcuBackupTestData.Gzip(CcuBackupTestData.CreateTar(
            (innerEntryName, "ids"u8.ToArray())));
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", userData);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task VerifyAsync_UsrLocalContentWithSimilarPrefix_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var userData = CcuBackupTestData.Gzip(CcuBackupTestData.CreateTar(
            ("usr/local-backup/etc/config/ids", "ids"u8.ToArray())));
        var backup = CcuBackupTestData.CreateBackupWith("usr_local.tar.gz", userData);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("usr_local.tar.gz");
    }

    [Fact]
    public async Task VerifyAsync_EntryNamesWithDifferentCasing_ThrowsInvalidFirmwareBackupException()
    {
        // Arrange
        var sut = new CcuBackupVerifier();
        var backup = CcuBackupTestData.CreateTar(
        [
            .. CcuBackupTestData.CreateValidEntries().Where(entry => entry.Name != "signature"),
            ("SIGNATURE", CcuBackupTestData.CreateValidSignature())
        ]);
        using var content = new MemoryStream(backup);

        // Act
        var act = async () => await sut.VerifyAsync(content);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidFirmwareBackupException>();
        ex.Which.Message.Should().Contain("signature");
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
