using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuClientLinkTests
{
    private const string SenderAddress = "BIDCOS:1";
    private const string ReceiverAddress = "BIDCOS:2";

    [Fact]
    public async Task GetAllLinksAsync_DefaultKind_CallsHomeMaticApiWithEmptyAddress()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var expected = new[] { new Link { Sender = SenderAddress, Receiver = ReceiverAddress } };
        A.CallTo(() => homeMaticApi.GetLinksAsync(string.Empty, (int)GetLinksFlags.None))
            .Returns(Task.FromResult<IEnumerable<Link>>(expected));

        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        var result = await client.GetAllLinksAsync();

        // Assert
        result.Should().BeEquivalentTo(expected);
        A.CallTo(() => homeMaticIpApi.GetLinksAsync(A<string>._, A<int>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GetAllLinksAsync_WithSpecificKind_CallsCorrespondingApi()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        const GetLinksFlags flags = GetLinksFlags.SenderParamSet;
        A.CallTo(() => homeMaticIpApi.GetLinksAsync(string.Empty, (int)flags))
            .Returns(Task.FromResult<IEnumerable<Link>>([]));

        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        await client.GetAllLinksAsync(CcuDeviceKind.HomeMaticIp, flags);

        // Assert
        A.CallTo(() => homeMaticIpApi.GetLinksAsync(string.Empty, (int)flags))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => homeMaticApi.GetLinksAsync(A<string>._, A<int>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task AddLinkAsync_DefaultKind_DelegatesToHomeMaticApi()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        await client.AddLinkAsync(SenderAddress, ReceiverAddress, "n", "d");

        // Assert
        A.CallTo(() => homeMaticApi.AddLinkAsync(SenderAddress, ReceiverAddress, "n", "d"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddLinkAsync_WithIpKind_DelegatesToHomeMaticIpApi()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        await client.AddLinkAsync(SenderAddress, ReceiverAddress, kind: CcuDeviceKind.HomeMaticIp);

        // Assert
        A.CallTo(() => homeMaticIpApi.AddLinkAsync(SenderAddress, ReceiverAddress, string.Empty, string.Empty))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => homeMaticApi.AddLinkAsync(A<string>._, A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null, "BIDCOS:2")]
    [InlineData("", "BIDCOS:2")]
    [InlineData("   ", "BIDCOS:2")]
    [InlineData("BIDCOS:1", null)]
    [InlineData("BIDCOS:1", "")]
    [InlineData("BIDCOS:1", "   ")]
    public async Task AddLinkAsync_NullOrWhitespaceAddress_ThrowsAndDoesNotCallApi(string? sender,
        string? receiver)
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        var act = () => client.AddLinkAsync(sender!, receiver!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(homeMaticApi).MustNotHaveHappened();
        A.CallTo(homeMaticIpApi).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddLinkAsync_NullName_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        var act = () => client.AddLinkAsync(SenderAddress, ReceiverAddress, null!, "d");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        A.CallTo(homeMaticApi).MustNotHaveHappened();
    }

    [Fact]
    public async Task RemoveLinkAsync_DelegatesToApiOfRequestedKind()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        await client.RemoveLinkAsync(SenderAddress, ReceiverAddress, CcuDeviceKind.HomeMaticIp);

        // Assert
        A.CallTo(() => homeMaticIpApi.RemoveLinkAsync(SenderAddress, ReceiverAddress))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetLinkInfoAsync_DelegatesToApi()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        await client.SetLinkInfoAsync(SenderAddress, ReceiverAddress, "name", "desc");

        // Assert
        A.CallTo(() => homeMaticApi.SetLinkInfoAsync(SenderAddress, ReceiverAddress, "name", "desc"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinkInfoAsync_ReturnsLinkInfoFromApiResponse()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => homeMaticApi.GetLinkInfoRawAsync(SenderAddress, ReceiverAddress))
            .Returns(Task.FromResult<IEnumerable<string>>(["the name", "the description"]));

        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        var info = await client.GetLinkInfoAsync(SenderAddress, ReceiverAddress);

        // Assert
        info.Name.Should().Be("the name");
        info.Description.Should().Be("the description");
    }

    [Theory]
    [InlineData(null, "BIDCOS:2")]
    [InlineData("", "BIDCOS:2")]
    [InlineData("BIDCOS:1", null)]
    [InlineData("BIDCOS:1", "")]
    public async Task RemoveLinkAsync_NullOrWhitespaceAddress_ThrowsAndDoesNotCallApi(string? sender,
        string? receiver)
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        var act = () => client.RemoveLinkAsync(sender!, receiver!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(homeMaticApi).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetLinkInfoAsync_NullName_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        var act = () => client.SetLinkInfoAsync(SenderAddress, ReceiverAddress, null!, "d");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        A.CallTo(homeMaticApi).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetLinkInfoAsync_NullSender_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpApi = A.Fake<IHomeMaticXmlRpcApi>();
        var client = CreateClient(homeMaticApi, homeMaticIpApi);

        // Act
        var act = () => client.GetLinkInfoAsync(null!, ReceiverAddress);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(homeMaticApi).MustNotHaveHappened();
    }

    [Fact]
    public async Task LinkOperation_UnknownDeviceKind_ThrowsKeyNotFound()
    {
        // Arrange
        var homeMaticApi = A.Fake<IHomeMaticXmlRpcApi>();
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var connection = new XmlRpcApiConnection(
            new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMatic),
            homeMaticApi);
        var xmlRpcApis = new Dictionary<CcuDeviceKind, XmlRpcApiConnection>
        {
            { CcuDeviceKind.HomeMatic, connection }
        };
        var client = new CcuClient(jsonRpcClient, xmlRpcApis,
            A.Fake<ICompleteCcuDeviceBuilder>());

        // Act
        var act = () => client.GetAllLinksAsync(CcuDeviceKind.HomeMaticIp);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static CcuClient CreateClient(IHomeMaticXmlRpcApi homeMaticApi,
        IHomeMaticXmlRpcApi homeMaticIpApi)
    {
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticConnection = new XmlRpcApiConnection(
            new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMatic),
            homeMaticApi);
        var homeMaticIpConnection = new XmlRpcApiConnection(
            new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMaticIp),
            homeMaticIpApi);

        var xmlRpcApis = new Dictionary<CcuDeviceKind, XmlRpcApiConnection>
        {
            { CcuDeviceKind.HomeMatic, homeMaticConnection },
            { CcuDeviceKind.HomeMaticIp, homeMaticIpConnection }
        };

        return new CcuClient(jsonRpcClient, xmlRpcApis, A.Fake<ICompleteCcuDeviceBuilder>());
    }
}
