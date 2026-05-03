using System.Text;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.EmbeddedResources.Extensions;

/// <summary>
/// Convenience helpers for reading and extracting embedded resources from an
/// <see cref="IEmbeddedResourceSet"/>.
/// </summary>
public static class EmbeddedResourceSetExtensions
{
    /// <summary>
    /// Reads the resource at <paramref name="relativePath"/> fully into a byte array.
    /// </summary>
    /// <param name="resources">The resource set to read from.</param>
    /// <param name="relativePath">A resource path relative to the resource root.</param>
    /// <returns>The full content of the resource as a byte array.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resources"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="FileNotFoundException">No resource exists at <paramref name="relativePath"/>.</exception>
    public static byte[] ReadAllBytes(this IEmbeddedResourceSet resources, string relativePath)
    {
        Ensure.NotNull(resources);
        Ensure.IsNotNullOrWhitespace(relativePath);

        using var stream = resources.Open(relativePath);
        using var memory = new MemoryStream();

        stream.CopyTo(memory);

        return memory.ToArray();
    }

    /// <summary>
    /// Reads the resource at <paramref name="relativePath"/> fully into a string.
    /// </summary>
    /// <param name="resources">The resource set to read from.</param>
    /// <param name="relativePath">A resource path relative to the resource root.</param>
    /// <param name="encoding">
    /// The text encoding to use. Defaults to <see cref="Encoding.UTF8"/> when
    /// <see langword="null"/>.
    /// </param>
    /// <returns>The decoded text content of the resource.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resources"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="FileNotFoundException">No resource exists at <paramref name="relativePath"/>.</exception>
    public static string ReadAllText(
        this IEmbeddedResourceSet resources,
        string relativePath,
        Encoding? encoding = null)
    {
        Ensure.NotNull(resources);
        Ensure.IsNotNullOrWhitespace(relativePath);

        using var stream = resources.Open(relativePath);
        using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);

        return reader.ReadToEnd();
    }

    /// <summary>
    /// Extracts a single embedded resource to disk at the given file path.
    /// </summary>
    /// <param name="resources">The resource set to read from.</param>
    /// <param name="relativePath">A source resource path relative to the resource root.</param>
    /// <param name="targetFilePath">
    /// The absolute or relative file path on disk where the resource should be written.
    /// </param>
    /// <param name="overwrite">
    /// <see langword="true"/> to replace an existing target file; otherwise,
    /// <see langword="false"/> to fail when the target already exists.
    /// </param>
    /// <returns>A <see cref="FileInfo"/> describing the written file.</returns>
    /// <remarks>Missing parent directories of <paramref name="targetFilePath"/> are created automatically.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="resources"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> or <paramref name="targetFilePath"/> is
    /// <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="FileNotFoundException">No resource exists at <paramref name="relativePath"/>.</exception>
    /// <exception cref="IOException">
    /// <paramref name="overwrite"/> is <see langword="false"/> and the target file already exists.
    /// </exception>
    public static FileInfo ExtractFile(
        this IEmbeddedResourceSet resources,
        string relativePath,
        string targetFilePath,
        bool overwrite = true)
    {
        Ensure.NotNull(resources);
        Ensure.IsNotNullOrWhitespace(relativePath);
        Ensure.IsNotNullOrWhitespace(targetFilePath);

        var resource = resources.Get(relativePath);
        var directory = Path.GetDirectoryName(targetFilePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!overwrite && File.Exists(targetFilePath))
        {
            throw new IOException($"File already exists: {targetFilePath}");
        }

        using (var source = resource.Open())
        using (var target = new FileStream(
            targetFilePath,
            overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None))
        {
            source.CopyTo(target);
        }

        return new FileInfo(targetFilePath);
    }

    /// <summary>
    /// Recursively extracts a resource subtree to disk, preserving the original folder
    /// structure beneath <paramref name="targetDirectory"/>.
    /// </summary>
    /// <param name="resources">The resource set to read from.</param>
    /// <param name="targetDirectory">
    /// The directory on disk that will receive the extracted files. Created if missing.
    /// </param>
    /// <param name="sourceSubdirectory">
    /// An optional subdirectory of the resource root to limit extraction to. The path of the
    /// subdirectory is stripped from the on-disk paths so files land directly under
    /// <paramref name="targetDirectory"/>.
    /// </param>
    /// <param name="overwrite">
    /// <see langword="true"/> to replace existing target files; otherwise,
    /// <see langword="false"/> to fail for any file that already exists.
    /// </param>
    /// <returns>A <see cref="DirectoryInfo"/> describing the target directory.</returns>
    /// <example>
    /// <code language="csharp">
    /// var resources = new EmbeddedResourceSet(typeof(MyMarker).Assembly);
    ///
    /// // Extract everything under Resources/ into /etc/app
    /// resources.ExtractTo("/etc/app");
    ///
    /// // Extract only Resources/templates into /etc/app
    /// resources.ExtractTo("/etc/app", "templates");
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="resources"/> or <paramref name="sourceSubdirectory"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="targetDirectory"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public static DirectoryInfo ExtractTo(
        this IEmbeddedResourceSet resources,
        string targetDirectory,
        string sourceSubdirectory = "",
        bool overwrite = true)
    {
        Ensure.NotNull(resources);
        Ensure.IsNotNullOrWhitespace(targetDirectory);
        Ensure.NotNull(sourceSubdirectory);

        Directory.CreateDirectory(targetDirectory);

        foreach (var resource in resources.Enumerate(sourceSubdirectory))
        {
            var relative = TrimSubdirectoryPrefix(resource.RelativePath, sourceSubdirectory);
            var targetPath = Path.Combine(targetDirectory, ToPlatformPath(relative));

            resources.ExtractFile(resource.RelativePath, targetPath, overwrite);
        }

        return new DirectoryInfo(targetDirectory);
    }

    private static string TrimSubdirectoryPrefix(string relativePath, string sourceSubdirectory)
    {
        if (string.IsNullOrWhiteSpace(sourceSubdirectory))
        {
            return relativePath;
        }

        var prefix = sourceSubdirectory.Replace('\\', '/').Trim().Trim('/') + "/";

        return relativePath.StartsWith(prefix, StringComparison.Ordinal)
            ? relativePath[prefix.Length..]
            : relativePath;
    }

    private static string ToPlatformPath(string forwardSlashPath)
    {
        return forwardSlashPath.Replace('/', Path.DirectorySeparatorChar);
    }
}
