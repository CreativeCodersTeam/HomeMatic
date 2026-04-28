using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using CreativeCoders.Net.XmlRpc.Proxy;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Client;

public class HomeMaticXmlRpcApiBuilderLinkSurfaceTests
{
    [Fact]
    public void Builder_BuildsProxy_WithLinkMethodsAvailable()
    {
        var proxyBuilder = A.Fake<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        var fakeApi = A.Fake<IHomeMaticXmlRpcApi>();

        A.CallTo(() => proxyBuilder.ForUrl(A<Uri>._)).Returns(proxyBuilder);
        A.CallTo(() => proxyBuilder.Build()).Returns(fakeApi);

        var sut = new HomeMaticXmlRpcApiBuilder(proxyBuilder);

        var api = sut.ForUrl(new Uri("http://localhost:2001/")).Build();

        api.Should().BeAssignableTo<IHomeMaticXmlRpcApi>();
    }

    [Fact]
    public void IHomeMaticXmlRpcApi_ExposesAllLinkMethods()
    {
        var type = typeof(IHomeMaticXmlRpcApi);

        type.GetMethod(nameof(IHomeMaticXmlRpcApi.GetLinksAsync)).Should().NotBeNull();
        type.GetMethod(nameof(IHomeMaticXmlRpcApi.AddLinkAsync)).Should().NotBeNull();
        type.GetMethod(nameof(IHomeMaticXmlRpcApi.RemoveLinkAsync)).Should().NotBeNull();
        type.GetMethod(nameof(IHomeMaticXmlRpcApi.SetLinkInfoAsync)).Should().NotBeNull();
        type.GetMethod(nameof(IHomeMaticXmlRpcApi.GetLinkInfoRawAsync)).Should().NotBeNull();
        type.GetMethod(nameof(IHomeMaticXmlRpcApi.ActivateLinkParamsetAsync)).Should().NotBeNull();
        type.GetMethod(nameof(IHomeMaticXmlRpcApi.GetLinkPeersAsync)).Should().NotBeNull();
    }
}
