using System.Formats.Tar;
using System.Globalization;
using System.IO.Compression;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.FirmwareBackup.Internal;

/// <summary>
/// Default <see cref="ICcuBackupVerifier"/>. Validates that a backup is a HomeMatic CCU backup archive:
/// an uncompressed tar containing a <c>signature</c> entry (32 hexadecimal characters), a
/// <c>usr_local.tar.gz</c> entry (a gzip compressed tar archive with <c>usr/local</c> content), a
/// <c>firmware_version</c> entry (<c>VERSION=&lt;version&gt;</c>) and a <c>key_index</c> entry
/// (a non-negative integer).
/// </summary>
/// <remarks>
/// The <c>signature</c> entry is only checked for its format; its bytes are not cryptographically
/// verified, so a successful verification does not prove the backup's authenticity.
/// </remarks>
internal sealed class CcuBackupVerifier : ICcuBackupVerifier
{
    private const string SignatureEntryName = "signature";

    private const string UsrLocalEntryName = "usr_local.tar.gz";

    private const string FirmwareVersionEntryName = "firmware_version";

    private const string KeyIndexEntryName = "key_index";

    private const string UsrLocalContentPrefix = "usr/local";

    private const string FirmwareVersionPrefix = "VERSION=";

    private const int SignatureLength = 32;

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

        var signatureValid = false;
        var usrLocalValid = false;
        var firmwareVersionValid = false;
        var keyIndexValid = false;

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
                        signatureValid = IsValidSignature(
                            await ReadEntryTextAsync(entry, cancellationToken).ConfigureAwait(false));
                        break;
                    case UsrLocalEntryName when entry.Length > 0:
                        usrLocalValid = await IsValidUsrLocalArchiveAsync(entry, cancellationToken)
                            .ConfigureAwait(false);
                        break;
                    case FirmwareVersionEntryName when entry.Length > 0:
                        firmwareVersionValid = IsValidFirmwareVersion(
                            await ReadEntryTextAsync(entry, cancellationToken).ConfigureAwait(false));
                        break;
                    case KeyIndexEntryName when entry.Length > 0:
                        keyIndexValid = IsValidKeyIndex(
                            await ReadEntryTextAsync(entry, cancellationToken).ConfigureAwait(false));
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

        if (!signatureValid)
        {
            throw new InvalidFirmwareBackupException(
                $"The downloaded backup is not a valid HomeMatic CCU backup: the '{SignatureEntryName}' entry is missing, empty or is not a valid backup signature.");
        }

        if (!usrLocalValid)
        {
            throw new InvalidFirmwareBackupException(
                $"The downloaded backup is not a valid HomeMatic CCU backup: the '{UsrLocalEntryName}' entry is missing, empty or is not a valid gzip compressed tar archive with '{UsrLocalContentPrefix}' content.");
        }

        if (!firmwareVersionValid)
        {
            throw new InvalidFirmwareBackupException(
                $"The downloaded backup is not a valid HomeMatic CCU backup: the '{FirmwareVersionEntryName}' entry is missing, empty or does not contain a valid firmware version.");
        }

        if (!keyIndexValid)
        {
            throw new InvalidFirmwareBackupException(
                $"The downloaded backup is not a valid HomeMatic CCU backup: the '{KeyIndexEntryName}' entry is missing, empty or does not contain a valid key index.");
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

    private static async Task<string?> ReadEntryTextAsync(TarEntry entry, CancellationToken cancellationToken)
    {
        var dataStream = entry.DataStream;

        if (dataStream is null)
        {
            return null;
        }

        // The entry was read with copyData: true, so its data stream is a seekable in-memory copy.
        dataStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(dataStream, leaveOpen: true);

        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        return text.Trim();
    }

    private static bool IsValidSignature(string? content)
    {
        return content is { Length: SignatureLength } && content.All(char.IsAsciiHexDigit);
    }

    private static bool IsValidFirmwareVersion(string? content)
    {
        if (content is null || !content.StartsWith(FirmwareVersionPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var version = content[FirmwareVersionPrefix.Length..];

        return version.Any(char.IsAsciiDigit)
               && version.All(static c => char.IsAsciiDigit(c) || c == '.');
    }

    private static bool IsValidKeyIndex(string? content)
    {
        return content is not null
               && int.TryParse(content, NumberStyles.None, CultureInfo.InvariantCulture, out _);
    }

    private static async Task<bool> IsValidUsrLocalArchiveAsync(TarEntry entry, CancellationToken cancellationToken)
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
            await using var innerTarReader = new TarReader(gzip, leaveOpen: true);

            var usrLocalContentFound = false;

            while (await innerTarReader.GetNextEntryAsync(copyData: false, cancellationToken).ConfigureAwait(false)
                   is { } innerEntry)
            {
                if (IsUsrLocalContentEntry(innerEntry.Name))
                {
                    usrLocalContentFound = true;
                }
            }

            // Drain the remaining gzip data so the trailing CRC is validated even when the tar
            // content ends before the compressed stream does.
            await gzip.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);

            return usrLocalContentFound;
        }
        catch (Exception ex) when (ex is InvalidDataException or EndOfStreamException or FormatException)
        {
            return false;
        }
    }

    private static bool IsUsrLocalContentEntry(string name)
    {
        var normalized = name.Replace('\\', '/').TrimStart('/');

        if (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.Equals(UsrLocalContentPrefix, StringComparison.Ordinal)
               || normalized.StartsWith(UsrLocalContentPrefix + "/", StringComparison.Ordinal);
    }
}
