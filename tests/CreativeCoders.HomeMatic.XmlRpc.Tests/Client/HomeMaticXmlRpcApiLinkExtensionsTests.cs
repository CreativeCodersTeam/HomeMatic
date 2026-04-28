using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Client;

public class HomeMaticXmlRpcApiLinkExtensionsTests
{
    [Fact]
    public async Task GetLinksAsync_TypedFlags_ForwardsBitmaskToApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinksAsync("ABC1234567:1", A<int>._))
            .Returns(Task.FromResult(Enumerable.Empty<Link>()));

        // Act
        await api.GetLinksAsync("ABC1234567:1",
            GetLinksFlags.Group | GetLinksFlags.SenderParamSet);

        // Assert
        A.CallTo(() => api.GetLinksAsync("ABC1234567:1", 3))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinksAsync_DefaultFlags_PassesZero()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinksAsync(A<string>._, A<int>._))
            .Returns(Task.FromResult(Enumerable.Empty<Link>()));

        // Act
        await api.GetLinksAsync("ABC1234567");

        // Assert
        A.CallTo(() => api.GetLinksAsync("ABC1234567", 0))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinksAsync_EmptyAddress_ForwardsEmptyAddressToApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinksAsync(A<string>._, A<int>._))
            .Returns(Task.FromResult(Enumerable.Empty<Link>()));

        // Act
        await api.GetLinksAsync(string.Empty, GetLinksFlags.Group);

        // Assert
        A.CallTo(() => api.GetLinksAsync(string.Empty, 1))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetLinksAsync_NullApi_ThrowsArgumentNullException()
    {
        // Arrange
        IHomeMaticXmlRpcApi api = null!;

        // Act
        Action act = () => api.GetLinksAsync("ABC1234567");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetLinksAsync_NullAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();

        // Act
        Action act = () => api.GetLinksAsync(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetLinkInfoAsync_TwoElementResponse_MapsToNameAndDescription()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>(["MyLink", "Some description"]));

        // Act
        var info = await api.GetLinkInfoAsync("S", "R");

        // Assert
        info.Name.Should().Be("MyLink");
        info.Description.Should().Be("Some description");
    }

    [Fact]
    public async Task GetLinkInfoAsync_EmptyResponse_ReturnsEmptyStrings()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>([]));

        // Act
        var info = await api.GetLinkInfoAsync("S", "R");

        // Assert
        info.Name.Should().BeEmpty();
        info.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLinkInfoAsync_SingleElementResponse_DescriptionEmpty()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>(["JustName"]));

        // Act
        var info = await api.GetLinkInfoAsync("S", "R");

        // Assert
        info.Name.Should().Be("JustName");
        info.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLinkInfoAsync_NullApi_ThrowsArgumentNullException()
    {
        // Arrange
        IHomeMaticXmlRpcApi api = null!;

        // Act
        Func<Task> act = () => api.GetLinkInfoAsync("S", "R");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetLinkInfoAsync_NullSenderAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();

        // Act
        Func<Task> act = () => api.GetLinkInfoAsync(null!, "R");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetLinkInfoAsync_NullReceiverAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();

        // Act
        Func<Task> act = () => api.GetLinkInfoAsync("S", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetLinkInfoAsync_RawAsyncReturnsNull_ReturnsEmptyStrings()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>(null!));

        // Act
        var info = await api.GetLinkInfoAsync("S", "R");

        // Assert
        info.Name.Should().BeEmpty();
        info.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLinkInfoAsync_RawEntriesAreNull_NormaliseToEmptyStrings()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>([null!, null!]));

        // Act
        var info = await api.GetLinkInfoAsync("S", "R");

        // Assert
        info.Name.Should().BeEmpty();
        info.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLinkInfoAsync_MoreThanTwoElements_IgnoresExtraEntries()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>(["Name", "Description", "Extra1", "Extra2"]));

        // Act
        var info = await api.GetLinkInfoAsync("S", "R");

        // Assert
        info.Name.Should().Be("Name");
        info.Description.Should().Be("Description");
    }
}
