using System.Formats.Tar;
using System.IO.Compression;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.FirmwareBackup.Internal;

/// <summary>
/// Default <see cref="ICcuBackupVerifier"/>. Validates that a backup is a HomeMatic CCU backup archive
/// (an uncompressed tar containing non-empty <c>signature</c> and <c>user_data.tar.gz</c> entries, the
/// latter being a valid gzip archive).
/// </summary>
/// <remarks>
/// The <c>signature</c> entry is only checked for presence and non-zero length; its bytes are not
/// cryptographically verified, so a successful verification does not prove the backup's authenticity.
/// </remarks>
internal sealed class CcuBackupVerifier : ICcuBackupVerifier
{
    private const string SignatureEntryName = "signature";

    private const string UserDataEntryName = "user_data.tar.gz";

    private const byte GzipId1 = 0x1F;

    private const byte GzipId2 = 0x8B;

    private const byte GzipDeflateMethod = 0x08;

    /// <inheritdoc />
    public async Task VerifyAsync(Stream content, CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(content);

        if (content.CanSeek)
        {
            content.Seek(0, SeekOrigin.Begin);
        }

        var signatureFound = false;
        var userDataValid = false;

        try
        {
            await using var tarReader = new TarReader(content, leaveOpen: true);

            while (await tarReader.GetNextEntryAsync(copyData: true, cancellationToken).ConfigureAwait(false)
                   is { } entry)
            {
                var entryName = NormalizeEntryName(entry.Name);

                switch (entryName)
                {
                    case SignatureEntryName when entry.Length > 0:
                        signatureFound = true;
                        break;
                    case UserDataEntryName when entry.Length > 0:
                        userDataValid = await IsValidGzipArchiveAsync(entry, cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidFirmwareBackupException(
                "The downloaded backup is not a valid HomeMatic CCU backup: it could not be read as a tar archive.",
                ex);
        }

        if (!signatureFound)
        {
            throw new InvalidFirmwareBackupException(
                $"The downloaded backup is not a valid HomeMatic CCU backup: the '{SignatureEntryName}' entry is missing or empty.");
        }

        if (!userDataValid)
        {
            throw new InvalidFirmwareBackupException(
                $"The downloaded backup is not a valid HomeMatic CCU backup: the '{UserDataEntryName}' entry is missing, empty or is not a valid gzip archive.");
        }
    }

    private static string NormalizeEntryName(string name)
    {
        var normalized = name.Replace('\\', '/');

        var lastSeparator = normalized.LastIndexOf('/');

        return lastSeparator < 0
            ? normalized
            : normalized[(lastSeparator + 1)..];
    }

    private static async Task<bool> IsValidGzipArchiveAsync(TarEntry entry, CancellationToken cancellationToken)
    {
        var dataStream = entry.DataStream;

        if (dataStream is null)
        {
            return false;
        }

        // The entry was read with copyData: true, so its data stream is a seekable in-memory copy.
        dataStream.Seek(0, SeekOrigin.Begin);

        var header = new byte[3];

        try
        {
            await dataStream.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        }
        catch (EndOfStreamException)
        {
            return false;
        }

        if (header[0] != GzipId1 || header[1] != GzipId2 || header[2] != GzipDeflateMethod)
        {
            return false;
        }

        dataStream.Seek(0, SeekOrigin.Begin);

        try
        {
            await using var gzip = new GZipStream(dataStream, CompressionMode.Decompress, leaveOpen: true);

            await gzip.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidDataException or EndOfStreamException)
        {
            return false;
        }

        return true;
    }
}
