using System.Formats.Tar;
using System.IO.Compression;

namespace CreativeCoders.HomeMatic.Tests.FirmwareBackup;

/// <summary>
/// Builds in-memory backup archives used to exercise HomeMatic CCU backup verification.
/// </summary>
internal static class CcuBackupTestData
{
    /// <summary>
    /// Creates a valid HomeMatic CCU backup: a tar archive containing a non-empty <c>signature</c>
    /// entry and a gzip-compressed <c>user_data.tar.gz</c> entry.
    /// </summary>
    /// <returns>The raw bytes of a valid backup archive.</returns>
    public static byte[] CreateValidBackup()
    {
        return CreateTar(
            ("signature", "signature-bytes"u8.ToArray()),
            ("user_data.tar.gz", Gzip("user-data-payload"u8.ToArray())));
    }

    /// <summary>
    /// Gzip-compresses the given bytes.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>The gzip-compressed bytes (starting with the gzip magic header).</returns>
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
    /// Builds a tar archive from the given entries.
    /// </summary>
    /// <param name="entries">The (entry name, content) pairs to write into the archive.</param>
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
