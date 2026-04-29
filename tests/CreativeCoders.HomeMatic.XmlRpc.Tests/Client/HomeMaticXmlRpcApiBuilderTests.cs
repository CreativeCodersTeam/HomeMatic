using System.Text;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.Net.XmlRpc.Proxy;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Client;

public class HomeMaticXmlRpcApiBuilderTests
{
    [Fact]
    public void Constructor_NullProxyBuilder_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new HomeMaticXmlRpcApiBuilder(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ForUrl_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        var proxyBuilder = A.Fake<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        var sut = new HomeMaticXmlRpcApiBuilder(proxyBuilder);

        // Act
        Action act = () => sut.ForUrl((Uri)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Build_AfterForUrl_DelegatesToProxyBuilderWithConfiguredUrl()
    {
        // Arrange
        var proxyBuilder = A.Fake<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        var fakeApi = A.Fake<IHomeMaticXmlRpcApi>();
        var url = new Uri("http://localhost:2001/");
        A.CallTo(() => proxyBuilder.UseEncoding(A<Encoding>._)).Returns(proxyBuilder);
        A.CallTo(() => proxyBuilder.ForUrl(url)).Returns(proxyBuilder);
        A.CallTo(() => proxyBuilder.Build()).Returns(fakeApi);
        var sut = new HomeMaticXmlRpcApiBuilder(proxyBuilder);

        // Act
        var api = sut.ForUrl(url).Build();

        // Assert
        api.Should().BeSameAs(fakeApi);
        A.CallTo(() => proxyBuilder.ForUrl(url)).MustHaveHappenedOnceExactly();
        A.CallTo(() => proxyBuilder.Build()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Build_WithoutForUrl_ThrowsInvalidOperationException()
    {
        // Arrange
        var proxyBuilder = A.Fake<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        var sut = new HomeMaticXmlRpcApiBuilder(proxyBuilder);

        // Act
        Action act = () => sut.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ForUrl_Uri_ReturnsSameBuilderInstance()
    {
        // Arrange
        var proxyBuilder = A.Fake<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        var sut = new HomeMaticXmlRpcApiBuilder(proxyBuilder);

        // Act
        var returned = sut.ForUrl(new Uri("http://localhost:2001/"));

        // Assert
        returned.Should().BeSameAs(sut);
    }

    [Fact]
    public void Build_AfterForUrlCalledTwice_UsesLastUrl()
    {
        // Arrange
        var proxyBuilder = A.Fake<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        var fakeApi = A.Fake<IHomeMaticXmlRpcApi>();
        var firstUrl = new Uri("http://first.local/");
        var secondUrl = new Uri("http://second.local/");
        A.CallTo(() => proxyBuilder.UseEncoding(A<Encoding>._)).Returns(proxyBuilder);
        A.CallTo(() => proxyBuilder.ForUrl(A<Uri>._)).Returns(proxyBuilder);
        A.CallTo(() => proxyBuilder.Build()).Returns(fakeApi);
        var sut = new HomeMaticXmlRpcApiBuilder(proxyBuilder);

        // Act
        sut.ForUrl(firstUrl).ForUrl(secondUrl).Build();

        // Assert
        A.CallTo(() => proxyBuilder.ForUrl(secondUrl)).MustHaveHappenedOnceExactly();
        A.CallTo(() => proxyBuilder.ForUrl(firstUrl)).MustNotHaveHappened();
    }

    [Fact]
    public void ForUrl_XmlRpcApiAddress_DelegatesToUriOverloadWithDerivedUrl()
    {
        // Arrange
        var proxyBuilder = A.Fake<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        var fakeApi = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => proxyBuilder.UseEncoding(A<Encoding>._)).Returns(proxyBuilder);
        A.CallTo(() => proxyBuilder.ForUrl(A<Uri>._)).Returns(proxyBuilder);
        A.CallTo(() => proxyBuilder.Build()).Returns(fakeApi);
        var apiAddress = new XmlRpcApiAddress(new Uri("http://192.168.1.100/"), CcuDeviceKind.HomeMatic);
        var expectedUrl = apiAddress.ToApiUrl();
        var sut = new HomeMaticXmlRpcApiBuilder(proxyBuilder);

        // Act
        sut.ForUrl(apiAddress).Build();

        // Assert
        A.CallTo(() => proxyBuilder.ForUrl(expectedUrl)).MustHaveHappenedOnceExactly();
    }
}
