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
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinksAsync("ABC1234567:1", A<int>._))
            .Returns(Task.FromResult(Enumerable.Empty<Link>()));

        await api.GetLinksAsync("ABC1234567:1",
            GetLinksFlags.Group | GetLinksFlags.SenderParamSet);

        A.CallTo(() => api.GetLinksAsync("ABC1234567:1", 3))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinksAsync_DefaultFlags_PassesZero()
    {
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinksAsync(A<string>._, A<int>._))
            .Returns(Task.FromResult(Enumerable.Empty<Link>()));

        await api.GetLinksAsync("ABC1234567");

        A.CallTo(() => api.GetLinksAsync("ABC1234567", 0))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinkInfoAsync_TwoElementResponse_MapsToNameAndDescription()
    {
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>(["MyLink", "Some description"]));

        var info = await api.GetLinkInfoAsync("S", "R");

        info.Name.Should().Be("MyLink");
        info.Description.Should().Be("Some description");
    }

    [Fact]
    public async Task GetLinkInfoAsync_EmptyResponse_ReturnsEmptyStrings()
    {
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>([]));

        var info = await api.GetLinkInfoAsync("S", "R");

        info.Name.Should().BeEmpty();
        info.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLinkInfoAsync_SingleElementResponse_DescriptionEmpty()
    {
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync("S", "R"))
            .Returns(Task.FromResult<IEnumerable<string>>(["JustName"]));

        var info = await api.GetLinkInfoAsync("S", "R");

        info.Name.Should().Be("JustName");
        info.Description.Should().BeEmpty();
    }
}
