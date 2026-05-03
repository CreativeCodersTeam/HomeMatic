using System.Text;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.EmbeddedResources.Extensions;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.EmbeddedResources.Tests;

public class EmbeddedResourceSetTests
{
    private static EmbeddedResourceSet CreateSut()
        => new(typeof(EmbeddedResourceSetTests).Assembly);

    [Fact]
    public void Enumerate_AllResources_ReturnsAllFilesWithForwardSlashPaths()
    {
        var sut = CreateSut();

        var paths = sut.Enumerate().Select(r => r.RelativePath).OrderBy(p => p).ToArray();

        paths.Should().BeEquivalentTo(
            "root.txt",
            "sub/deep/leaf.json",
            "sub/nested.txt");
    }

    [Fact]
    public void Enumerate_Subdirectory_ReturnsOnlyFilesInThatSubtree()
    {
        var sut = CreateSut();

        var paths = sut.Enumerate("sub").Select(r => r.RelativePath).OrderBy(p => p).ToArray();

        paths.Should().BeEquivalentTo(
            "sub/deep/leaf.json",
            "sub/nested.txt");
    }

    [Fact]
    public void ReadAllText_ExistingResource_ReturnsExpectedContent()
    {
        var sut = CreateSut();

        var text = sut.ReadAllText("root.txt");

        text.Should().Contain("root content");
    }

    [Fact]
    public void Get_MissingResource_ThrowsFileNotFoundException()
    {
        var sut = CreateSut();

        var act = () => sut.Get("missing.txt");

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Find_MissingResource_ReturnsNull()
    {
        var sut = CreateSut();

        var resource = sut.Find("missing.txt");

        resource.Should().BeNull();
    }

    [Fact]
    public void Exists_ExistingResource_ReturnsTrue()
    {
        var sut = CreateSut();

        sut.Exists("root.txt").Should().BeTrue();
        sut.Exists("missing.txt").Should().BeFalse();
    }

    [Fact]
    public void ExtractFile_WritesSingleFile()
    {
        var sut = CreateSut();
        var tempDir = CreateTempDir();
        var target = Path.Combine(tempDir, "out.txt");

        try
        {
            sut.ExtractFile("root.txt", target);

            File.Exists(target).Should().BeTrue();
            File.ReadAllText(target).Should().Be(sut.ReadAllText("root.txt"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ExtractFile_WithoutOverwrite_ThrowsWhenTargetExists()
    {
        var sut = CreateSut();
        var tempDir = CreateTempDir();
        var target = Path.Combine(tempDir, "out.txt");

        try
        {
            File.WriteAllText(target, "existing");

            var act = () => sut.ExtractFile("root.txt", target, overwrite: false);

            act.Should().Throw<IOException>();
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ExtractTo_FullSet_RecreatesDirectoryStructure()
    {
        var sut = CreateSut();
        var tempDir = CreateTempDir();

        try
        {
            sut.ExtractTo(tempDir);

            File.Exists(Path.Combine(tempDir, "root.txt")).Should().BeTrue();
            File.Exists(Path.Combine(tempDir, "sub", "nested.txt")).Should().BeTrue();
            File.Exists(Path.Combine(tempDir, "sub", "deep", "leaf.json")).Should().BeTrue();

            File.ReadAllBytes(Path.Combine(tempDir, "sub", "deep", "leaf.json"))
                .Should().Equal(sut.ReadAllBytes("sub/deep/leaf.json"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ExtractTo_Subdirectory_ExtractsOnlySubtreeStrippingPrefix()
    {
        var sut = CreateSut();
        var tempDir = CreateTempDir();

        try
        {
            sut.ExtractTo(tempDir, "sub");

            File.Exists(Path.Combine(tempDir, "nested.txt")).Should().BeTrue();
            File.Exists(Path.Combine(tempDir, "deep", "leaf.json")).Should().BeTrue();
            File.Exists(Path.Combine(tempDir, "root.txt")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Open_AcceptsBackslashAndForwardSlashEquivalently()
    {
        var sut = CreateSut();

        using var forward = sut.Open("sub/nested.txt");
        using var backward = sut.Open("sub\\nested.txt");

        ReadAll(forward).Should().Be(ReadAll(backward));
    }

    [Fact]
    public void Ctor_NullAssembly_ThrowsArgumentNullException()
    {
        // Arrange / Act
        var act = () => new EmbeddedResourceSet(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidResourcesRoot_ThrowsArgumentException(string? root)
    {
        // Arrange / Act
        var act = () => new EmbeddedResourceSet(typeof(EmbeddedResourceSetTests).Assembly, root!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Exists_InvalidPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.Exists(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Get_InvalidPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.Get(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Find_InvalidPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.Find(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Open_InvalidPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.Open(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Enumerate_NullDirectory_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.Enumerate(null!).ToArray();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Enumerate_NonExistentSubdirectory_ReturnsEmpty()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var resources = sut.Enumerate("does/not/exist").ToArray();

        // Assert
        resources.Should().BeEmpty();
    }

    [Fact]
    public void Enumerate_CalledMultipleTimes_ReturnsSameResults()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var first = sut.Enumerate().Select(r => r.RelativePath).OrderBy(p => p).ToArray();
        var second = sut.Enumerate().Select(r => r.RelativePath).OrderBy(p => p).ToArray();

        // Assert
        second.Should().Equal(first);
    }

    [Fact]
    public void Enumerate_BackslashSubdirectory_EquivalentToForwardSlash()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var forward = sut.Enumerate("sub").Select(r => r.RelativePath).OrderBy(p => p).ToArray();
        var backward = sut.Enumerate("sub\\").Select(r => r.RelativePath).OrderBy(p => p).ToArray();

        // Assert
        backward.Should().Equal(forward);
    }

    [Fact]
    public void Find_LeadingSlashInPath_ResolvesSameAsWithout()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var withSlash = sut.Find("/root.txt");
        var withoutSlash = sut.Find("root.txt");

        // Assert
        withSlash.Should().NotBeNull();
        withoutSlash.Should().NotBeNull();
        withSlash!.RelativePath.Should().Be(withoutSlash!.RelativePath);
    }

    [Fact]
    public void ReadAllBytes_ReturnsByteEqualContent()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var bytes = sut.ReadAllBytes("sub/deep/leaf.json");
        var text = Encoding.UTF8.GetString(bytes);

        // Assert
        text.Should().Contain("\"leaf\": true");
    }

    [Fact]
    public void ReadAllText_WithCustomEncoding_DecodesUsingThatEncoding()
    {
        // Arrange
        var sut = CreateSut();
        var bytes = sut.ReadAllBytes("root.txt");

        // Act
        var explicitAscii = sut.ReadAllText("root.txt", Encoding.ASCII);
        var defaultUtf8 = sut.ReadAllText("root.txt");

        // Assert
        explicitAscii.Should().Be(Encoding.ASCII.GetString(bytes));
        defaultUtf8.Should().Be(Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void ExtractTo_OverwriteFalse_ThrowsWhenTargetExists()
    {
        // Arrange
        var sut = CreateSut();
        var tempDir = CreateTempDir();
        var collidingFile = Path.Combine(tempDir, "root.txt");

        try
        {
            File.WriteAllText(collidingFile, "pre-existing");

            // Act
            var act = () => sut.ExtractTo(tempDir, overwrite: false);

            // Assert
            act.Should().Throw<IOException>();
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ExtractTo_OverwriteTrue_ReplacesExistingFile()
    {
        // Arrange
        var sut = CreateSut();
        var tempDir = CreateTempDir();
        var collidingFile = Path.Combine(tempDir, "root.txt");

        try
        {
            File.WriteAllText(collidingFile, "pre-existing");

            // Act
            sut.ExtractTo(tempDir, overwrite: true);

            // Assert
            File.ReadAllText(collidingFile).Should().Be(sut.ReadAllText("root.txt"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ReadAllBytes_NullResources_ThrowsArgumentNullException()
    {
        // Arrange / Act
        var act = () => EmbeddedResourceSetExtensions.ReadAllBytes(null!, "root.txt");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReadAllBytes_InvalidPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var resources = A.Fake<IEmbeddedResourceSet>();

        // Act
        var act = () => resources.ReadAllBytes(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReadAllText_NullResources_ThrowsArgumentNullException()
    {
        // Arrange / Act
        var act = () => EmbeddedResourceSetExtensions.ReadAllText(null!, "root.txt");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReadAllText_InvalidPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var resources = A.Fake<IEmbeddedResourceSet>();

        // Act
        var act = () => resources.ReadAllText(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExtractFile_NullResources_ThrowsArgumentNullException()
    {
        // Arrange / Act
        var act = () => EmbeddedResourceSetExtensions.ExtractFile(null!, "root.txt", "/tmp/out.txt");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractFile_InvalidRelativePath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var resources = A.Fake<IEmbeddedResourceSet>();

        // Act
        var act = () => resources.ExtractFile(path!, "/tmp/out.txt");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractFile_InvalidTargetPath_ThrowsArgumentException(string? target)
    {
        // Arrange
        var resources = A.Fake<IEmbeddedResourceSet>();

        // Act
        var act = () => resources.ExtractFile("root.txt", target!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExtractTo_NullResources_ThrowsArgumentNullException()
    {
        // Arrange / Act
        var act = () => EmbeddedResourceSetExtensions.ExtractTo(null!, "/tmp");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractTo_InvalidTargetDirectory_ThrowsArgumentException(string? target)
    {
        // Arrange
        var resources = A.Fake<IEmbeddedResourceSet>();

        // Act
        var act = () => resources.ExtractTo(target!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExtractTo_NullSourceSubdirectory_ThrowsArgumentNullException()
    {
        // Arrange
        var resources = A.Fake<IEmbeddedResourceSet>();

        // Act
        var act = () => resources.ExtractTo("/tmp", sourceSubdirectory: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "emb-res-tests-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(dir);

        return dir;
    }

    private static string ReadAll(Stream stream)
    {
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
