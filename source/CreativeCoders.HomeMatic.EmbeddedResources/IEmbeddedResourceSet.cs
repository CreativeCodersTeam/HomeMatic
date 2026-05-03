namespace CreativeCoders.HomeMatic.EmbeddedResources;

/// <summary>
/// Provides path-based read access to a set of embedded files shipped with an assembly,
/// preserving the original folder structure.
/// </summary>
/// <remarks>
/// Path arguments accept both forward (<c>/</c>) and backward (<c>\</c>) slashes; both are
/// normalized to forward slashes internally. Returned <see cref="EmbeddedResource.RelativePath"/>
/// values always use forward slashes.
/// </remarks>
public interface IEmbeddedResourceSet
{
    /// <summary>
    /// Recursively enumerates all embedded resources beneath the given directory.
    /// </summary>
    /// <param name="relativeDirectory">
    /// A directory path relative to the resource root. An empty string enumerates everything.
    /// </param>
    /// <returns>A lazy sequence of resource entries beneath the given directory.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="relativeDirectory"/> is <see langword="null"/>.
    /// </exception>
    IEnumerable<EmbeddedResource> Enumerate(string relativeDirectory = "");

    /// <summary>
    /// Determines whether an embedded resource exists at the given relative path.
    /// </summary>
    /// <param name="relativePath">A resource path relative to the resource root.</param>
    /// <returns>
    /// <see langword="true"/> if a file is present at <paramref name="relativePath"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    bool Exists(string relativePath);

    /// <summary>
    /// Returns the embedded resource at the given relative path.
    /// </summary>
    /// <param name="relativePath">A resource path relative to the resource root.</param>
    /// <returns>The matching resource entry.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="FileNotFoundException">No resource exists at <paramref name="relativePath"/>.</exception>
    EmbeddedResource Get(string relativePath);

    /// <summary>
    /// Returns the embedded resource at the given relative path, or <see langword="null"/> if
    /// no such resource exists.
    /// </summary>
    /// <param name="relativePath">A resource path relative to the resource root.</param>
    /// <returns>The matching resource entry, or <see langword="null"/> when missing.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    EmbeddedResource? Find(string relativePath);

    /// <summary>
    /// Opens a read-only stream over the embedded resource at the given path.
    /// </summary>
    /// <param name="relativePath">A resource path relative to the resource root.</param>
    /// <returns>A read-only stream positioned at the start of the resource content.</returns>
    /// <remarks>The caller owns the returned stream and is responsible for disposing it.</remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="FileNotFoundException">No resource exists at <paramref name="relativePath"/>.</exception>
    Stream Open(string relativePath);
}
