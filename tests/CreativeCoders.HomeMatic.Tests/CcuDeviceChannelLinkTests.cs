using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuDeviceChannelLinkTests
{
    private const string ChannelAddress = "BIDCOS:1";
    private const string ReceiverAddress = "BIDCOS:2";

    [Fact]
    public async Task GetLinksAsync_WithDefaultFlags_PassesChannelAddressAndZeroFlagsToApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var expected = new[] { new Link { Sender = ChannelAddress, Receiver = ReceiverAddress } };
        A.CallTo(() => api.GetLinksAsync(ChannelAddress, (int)GetLinksFlags.None))
            .Returns(Task.FromResult<IEnumerable<Link>>(expected));

        var channel = CreateChannel(api);

        // Act
        var result = await channel.GetLinksAsync();

        // Assert
        result.Should().BeEquivalentTo(expected);
        A.CallTo(() => api.GetLinksAsync(ChannelAddress, (int)GetLinksFlags.None))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinksAsync_WithFlags_ForwardsFlagsAsIntToApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        const GetLinksFlags flags = GetLinksFlags.Group | GetLinksFlags.SenderParamSet;
        A.CallTo(() => api.GetLinksAsync(ChannelAddress, (int)flags))
            .Returns(Task.FromResult<IEnumerable<Link>>([]));

        var channel = CreateChannel(api);

        // Act
        await channel.GetLinksAsync(flags);

        // Assert
        A.CallTo(() => api.GetLinksAsync(ChannelAddress, (int)flags))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinkPeersAsync_PassesChannelAddressToApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var peers = new[] { ReceiverAddress, "BIDCOS:3" };
        A.CallTo(() => api.GetLinkPeersAsync(ChannelAddress))
            .Returns(Task.FromResult<IEnumerable<string>>(peers));

        var channel = CreateChannel(api);

        // Act
        var result = await channel.GetLinkPeersAsync();

        // Assert
        result.Should().BeEquivalentTo(peers);
    }

    [Fact]
    public async Task AddLinkToAsync_DelegatesToApiUsingChannelAddressAsSender()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        await channel.AddLinkToAsync(ReceiverAddress, "name", "description");

        // Assert
        A.CallTo(() => api.AddLinkAsync(ChannelAddress, ReceiverAddress, "name", "description"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddLinkToAsync_WithoutNameAndDescription_DefaultsToEmptyStrings()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        await channel.AddLinkToAsync(ReceiverAddress);

        // Assert
        A.CallTo(() => api.AddLinkAsync(ChannelAddress, ReceiverAddress, string.Empty, string.Empty))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddLinkToAsync_NullOrWhitespaceReceiver_ThrowsAndDoesNotCallApi(string? receiver)
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.AddLinkToAsync(receiver!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddLinkToAsync_NullName_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.AddLinkToAsync(ReceiverAddress, null!, "desc");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddLinkToAsync_NullDescription_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.AddLinkToAsync(ReceiverAddress, "name", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    [Fact]
    public async Task RemoveLinkToAsync_DelegatesToApiUsingChannelAddressAsSender()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        await channel.RemoveLinkToAsync(ReceiverAddress);

        // Assert
        A.CallTo(() => api.RemoveLinkAsync(ChannelAddress, ReceiverAddress))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveLinkToAsync_NullReceiver_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.RemoveLinkToAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetLinkInfoAsync_NullReceiver_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.SetLinkInfoAsync(null!, "n", "d");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetLinkInfoAsync_NullName_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.SetLinkInfoAsync(ReceiverAddress, null!, "d");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetLinkInfoAsync_NullReceiver_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.GetLinkInfoAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetLinksAsync_ApiThrows_ExceptionPropagates()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinksAsync(A<string>._, A<int>._))
            .Throws(new InvalidOperationException("boom"));
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.GetLinksAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task SetLinkInfoAsync_DelegatesToApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        await channel.SetLinkInfoAsync(ReceiverAddress, "new name", "new description");

        // Assert
        A.CallTo(() => api.SetLinkInfoAsync(ChannelAddress, ReceiverAddress, "new name", "new description"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLinkInfoAsync_ReturnsLinkInfoFromApiResponse()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetLinkInfoRawAsync(ChannelAddress, ReceiverAddress))
            .Returns(Task.FromResult<IEnumerable<string>>(["the name", "the description"]));

        var channel = CreateChannel(api);

        // Act
        var info = await channel.GetLinkInfoAsync(ReceiverAddress);

        // Assert
        info.Name.Should().Be("the name");
        info.Description.Should().Be("the description");
    }

    [Fact]
    public async Task ActivateLinkParamsetAsync_DelegatesToApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        await channel.ActivateLinkParamsetAsync(ReceiverAddress, longPress: true);

        // Assert
        A.CallTo(() => api.ActivateLinkParamsetAsync(ChannelAddress, ReceiverAddress, true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ActivateLinkParamsetAsync_NullPeer_ThrowsAndDoesNotCallApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel = CreateChannel(api);

        // Act
        var act = () => channel.ActivateLinkParamsetAsync(null!, longPress: false);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        A.CallTo(api).MustNotHaveHappened();
    }

    private static CcuDeviceChannel CreateChannel(IHomeMaticXmlRpcApi api)
    {
        return new CcuDeviceChannel(api)
        {
            Uri = new CcuDeviceUri
            {
                CcuHost = "localhost",
                Kind = CcuDeviceKind.HomeMatic,
                Address = ChannelAddress
            },
            DeviceType = "TestChannel",
            IsAesActive = false,
            Interface = "BidCos-RF",
            Version = 1,
            Roaming = false,
            ParamSets = ["MASTER", "VALUES"],
            Index = 1,
            Group = string.Empty,
            ChannelDirection = ChannelDirection.None
        };
    }
}
