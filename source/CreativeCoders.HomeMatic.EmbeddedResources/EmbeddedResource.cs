using CreativeCoders.Core;
using Microsoft.Extensions.FileProviders;

namespace CreativeCoders.HomeMatic.EmbeddedResources;

/// <summary>
/// Represents a single embedded resource entry exposed by an
/// <see cref="IEmbeddedResourceSet"/>.
/// </summary>
public sealed class EmbeddedResource
{
    private readonly IFileInfo _fileInfo;

    internal EmbeddedResource(string relativePath, IFileInfo fileInfo)
    {
        Ensure.IsNotNullOrWhitespace(relativePath);
        Ensure.NotNull(fileInfo);

        RelativePath = relativePath;
        _fileInfo = fileInfo;
    }

    /// <summary>
    /// Gets the path of this resource relative to the resource root.
    /// </summary>
    /// <value>A path that uses forward slashes (<c>/</c>) as separators.</value>
    public string RelativePath { get; }

    /// <summary>
    /// Gets the file name component of this resource.
    /// </summary>
    /// <value>The leaf file name without any directory portion.</value>
    public string Name => _fileInfo.Name;

    /// <summary>
    /// Gets the size of this resource in bytes.
    /// </summary>
    /// <value>The length of the resource content in bytes.</value>
    public long Length => _fileInfo.Length;

    /// <summary>
    /// Opens a read-only stream over the contents of this resource.
    /// </summary>
    /// <returns>A read-only stream positioned at the start of the resource content.</returns>
    /// <remarks>
    /// The caller owns the returned stream and is responsible for disposing it. Each call
    /// returns a fresh stream; multiple readers can therefore be active concurrently.
    /// </remarks>
    public Stream Open()
    {
        return _fileInfo.CreateReadStream();
    }
}
