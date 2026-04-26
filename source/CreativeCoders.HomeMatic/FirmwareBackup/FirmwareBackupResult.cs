using CreativeCoders.Core;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Represents the result of a firmware backup download from a HomeMatic CCU.
/// </summary>
/// <remarks>
/// The instance owns the underlying <see cref="Stream"/> and any HTTP resources required to read it.
/// Always dispose the result to release the connection.
/// </remarks>
[PublicAPI]
public sealed class FirmwareBackupResult : IAsyncDisposable, IDisposable
{
    private readonly IAsyncDisposable[] _additionalResources;

    /// <summary>
    /// Initializes a new instance of <see cref="FirmwareBackupResult"/>.
    /// </summary>
    /// <param name="content">Stream containing the backup payload.</param>
    /// <param name="fileName">Suggested file name for the backup (e.g. <c>ccu_backup.sbk</c>).</param>
    /// <param name="contentLength">Optional content length in bytes if reported by the server.</param>
    /// <param name="additionalResources">
    /// Optional additional resources (such as the underlying <see cref="HttpResponseMessage"/>) that need
    /// to be disposed together with the content stream.
    /// </param>
    public FirmwareBackupResult(
        Stream content,
        string fileName,
        long? contentLength = null,
        params IAsyncDisposable[] additionalResources)
    {
        Content = Ensure.NotNull(content);
        FileName = Ensure.IsNotNullOrWhitespace(fileName);
        ContentLength = contentLength;
        _additionalResources = additionalResources ?? [];
    }

    /// <summary>
    /// Gets the stream that contains the backup payload.
    /// </summary>
    public Stream Content { get; }

    /// <summary>
    /// Gets the suggested file name for the downloaded backup.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the content length in bytes if reported by the CCU; otherwise <see langword="null"/>.
    /// </summary>
    public long? ContentLength { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync().ConfigureAwait(false);

        foreach (var resource in _additionalResources)
        {
            await resource.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
