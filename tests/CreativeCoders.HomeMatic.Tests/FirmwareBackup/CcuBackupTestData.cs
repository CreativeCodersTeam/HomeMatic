using System.Formats.Tar;
using System.IO.Compression;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

/// <summary>
/// Builds in-memory backup archives used to exercise HomeMatic CCU backup verification.
/// </summary>
internal static class CcuBackupTestData
{
    /// <summary>
    /// Creates a valid HomeMatic CCU backup: a tar archive containing well-formed <c>signature</c>,
    /// <c>usr_local.tar.gz</c>, <c>firmware_version</c> and <c>key_index</c> entries, mirroring a
    /// real CCU3 <c>.sbk</c> file.
    /// </summary>
    /// <returns>The raw bytes of a valid backup archive.</returns>
    public static byte[] CreateValidBackup()
    {
        return CreateTar(CreateValidEntries());
    }

    /// <summary>
    /// Creates the entry set of a valid HomeMatic CCU backup.
    /// </summary>
    /// <returns>The named entries of a valid backup archive.</returns>
    public static (string Name, byte[] Content)[] CreateValidEntries()
    {
        return
        [
            ("signature", CreateValidSignature()),
            ("usr_local.tar.gz", CreateValidUserData()),
            ("firmware_version", "VERSION=3.83.6\n"u8.ToArray()),
            ("key_index", "0\n"u8.ToArray())
        ];
    }

    /// <summary>
    /// Creates a backup that is valid except that the entry with the given name is missing.
    /// </summary>
    /// <param name="entryName">The name of the entry to omit.</param>
    /// <returns>The raw bytes of the backup archive.</returns>
    public static byte[] CreateBackupWithout(string entryName)
    {
        return CreateTar(CreateValidEntries().Where(entry => entry.Name != entryName).ToArray());
    }

    /// <summary>
    /// Creates a backup that is valid except that the entry with the given name has the given content.
    /// </summary>
    /// <param name="entryName">The name of the entry to replace.</param>
    /// <param name="content">The content to use for the entry.</param>
    /// <returns>The raw bytes of the backup archive.</returns>
    public static byte[] CreateBackupWith(string entryName, byte[] content)
    {
        return CreateTar(
            CreateValidEntries()
                .Select(entry => entry.Name == entryName ? (entry.Name, content) : entry)
                .ToArray());
    }

    /// <summary>
    /// Creates a well-formed backup signature: 32 hexadecimal characters followed by a newline.
    /// </summary>
    /// <returns>The raw bytes of the signature entry.</returns>
    public static byte[] CreateValidSignature()
    {
        return "d37893be27a7d3642fe6ff8b9ea78bc5\n"u8.ToArray();
    }

    /// <summary>
    /// Creates a valid <c>usr_local.tar.gz</c> payload: a gzip-compressed tar archive whose
    /// entries live under <c>usr/local/</c>.
    /// </summary>
    /// <returns>The raw bytes of the user data entry.</returns>
    public static byte[] CreateValidUserData()
    {
        return Gzip(CreateTar(
            ("usr/local/etc/config/homematic.regadom", "rega-config"u8.ToArray()),
            ("usr/local/etc/config/ids", "ids"u8.ToArray())));
    }

    /// <summary>
    /// Compresses the given data using gzip.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>The gzip-compressed data.</returns>
    public static byte[] Gzip(byte[] data)
    {
        using var output = new MemoryStream();

        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(data);
        }

        return output.ToArray();
    }

    /// <summary>
    /// Creates a tar archive containing the given entries.
    /// </summary>
    /// <param name="entries">The named entries to write into the archive.</param>
    /// <returns>The raw bytes of the tar archive.</returns>
    public static byte[] CreateTar(params (string Name, byte[] Content)[] entries)
    {
        using var output = new MemoryStream();

        using (var writer = new TarWriter(output, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = new PaxTarEntry(TarEntryType.RegularFile, name)
                {
                    DataStream = new MemoryStream(content)
                };

                writer.WriteEntry(entry);
            }
        }

        return output.ToArray();
    }
}
