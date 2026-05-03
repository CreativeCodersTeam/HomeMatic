using System.Reflection;
using CreativeCoders.Core;
using Microsoft.Extensions.FileProviders;

namespace CreativeCoders.HomeMatic.EmbeddedResources;

/// <summary>
/// Default implementation of <see cref="IEmbeddedResourceSet"/> that exposes embedded resources
/// of an assembly via a <see cref="ManifestEmbeddedFileProvider"/>, preserving the original
/// folder structure.
/// </summary>
/// <remarks>
/// The consuming project must enable the embedded files manifest (set the MSBuild property
/// <c>GenerateEmbeddedFilesManifest</c> to <see langword="true"/>) and reference
/// <c>Microsoft.Extensions.FileProviders.Embedded</c> directly so that its build targets run.
/// Build targets from NuGet packages are not propagated through transitive
/// <c>ProjectReference</c> chains.
/// </remarks>
/// <example>
/// <code language="csharp">
/// var resources = new EmbeddedResourceSet(typeof(MyMarker).Assembly);
///
/// foreach (var entry in resources.Enumerate())
/// {
///     Console.WriteLine(entry.RelativePath);
/// }
///
/// resources.ExtractTo("/etc/app");
/// </code>
/// </example>
public class EmbeddedResourceSet : IEmbeddedResourceSet
{
    private readonly IFileProvider _fileProvider;

    private readonly string _root;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedResourceSet"/> class that reads
    /// the embedded files manifest of the given <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">The assembly that contains the embedded resources.</param>
    /// <param name="resourcesRoot">
    /// The relative folder inside the originating project that was embedded as the resource
    /// root. Defaults to <c>Resources</c>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="resourcesRoot"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public EmbeddedResourceSet(Assembly assembly, string resourcesRoot = "Resources")
    {
        Ensure.NotNull(assembly);
        Ensure.IsNotNullOrWhitespace(resourcesRoot);

        _fileProvider = new ManifestEmbeddedFileProvider(assembly, resourcesRoot);
        _root = resourcesRoot;
    }

    internal EmbeddedResourceSet(IFileProvider fileProvider, string resourcesRoot)
    {
        Ensure.NotNull(fileProvider);
        Ensure.IsNotNullOrWhitespace(resourcesRoot);

        _fileProvider = fileProvider;
        _root = resourcesRoot;
    }

    /// <inheritdoc/>
    public IEnumerable<EmbeddedResource> Enumerate(string relativeDirectory = "")
    {
        Ensure.NotNull(relativeDirectory);

        var startDir = NormalizeDirectory(relativeDirectory);

        return EnumerateRecursive(startDir);
    }

    /// <inheritdoc/>
    public bool Exists(string relativePath)
    {
        Ensure.IsNotNullOrWhitespace(relativePath);

        var normalized = NormalizePath(relativePath);
        var info = _fileProvider.GetFileInfo(normalized);

        return info.Exists && !info.IsDirectory;
    }

    /// <inheritdoc/>
    public EmbeddedResource Get(string relativePath)
    {
        Ensure.IsNotNullOrWhitespace(relativePath);

        return Find(relativePath)
            ?? throw new FileNotFoundException(
                $"Embedded resource '{relativePath}' was not found in '{_root}'.",
                relativePath);
    }

    /// <inheritdoc/>
    public EmbeddedResource? Find(string relativePath)
    {
        Ensure.IsNotNullOrWhitespace(relativePath);

        var normalized = NormalizePath(relativePath);
        var info = _fileProvider.GetFileInfo(normalized);

        if (!info.Exists || info.IsDirectory)
        {
            return null;
        }

        return new EmbeddedResource(TrimLeadingSlash(normalized), info);
    }

    /// <inheritdoc/>
    public Stream Open(string relativePath)
    {
        Ensure.IsNotNullOrWhitespace(relativePath);

        return Get(relativePath).Open();
    }

    private IEnumerable<EmbeddedResource> EnumerateRecursive(string directory)
    {
        var contents = _fileProvider.GetDirectoryContents(directory);

        if (!contents.Exists)
        {
            yield break;
        }

        foreach (var entry in contents)
        {
            var childPath = CombinePath(directory, entry.Name);

            if (entry.IsDirectory)
            {
                foreach (var nested in EnumerateRecursive(childPath))
                {
                    yield return nested;
                }

                continue;
            }

            yield return new EmbeddedResource(TrimLeadingSlash(childPath), entry);
        }
    }

    internal static string NormalizePath(string relativePath)
    {
        var replaced = relativePath.Replace('\\', '/').Trim();

        return replaced.StartsWith('/') ? replaced : "/" + replaced;
    }

    private static string NormalizeDirectory(string relativeDirectory)
    {
        if (string.IsNullOrWhiteSpace(relativeDirectory))
        {
            return "/";
        }

        var replaced = relativeDirectory.Replace('\\', '/').Trim().TrimEnd('/');

        return replaced.StartsWith('/') ? replaced : "/" + replaced;
    }

    private static string CombinePath(string directory, string name)
    {
        if (directory == "/" || directory.Length == 0)
        {
            return "/" + name;
        }

        return directory + "/" + name;
    }

    private static string TrimLeadingSlash(string path)
    {
        return path.StartsWith('/') ? path[1..] : path;
    }
}
