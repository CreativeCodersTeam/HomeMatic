using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuClientTests
{
    [Fact]
    public async Task GetDevicesAsync_NoDevicesAvailable_ReturnsEmptyArray()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));

        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        // Act
        var devices = await ccuClient.GetDevicesAsync();

        // Assert
        devices
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task GetDevicesAsync_HomeMaticDevicesAvailable_ReturnsCcuDevices()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        IEnumerable<DeviceDescription> homeMaticDevices =
        [
            new DeviceDescription
            {
                Address = "1234567890",
                Interface = "BidCos-RF"
            },
            new DeviceDescription
            {
                Address = "1234567891",
                Interface = "BidCos-RF"
            }
        ];

        IEnumerable<DeviceDetails> homeMaticDeviceDetails =
        [
            new DeviceDetails
            {
                Address = "1234567890",
                Interface = "BidCos-RF",
                Type = "HmIP-SWDO",
                Name = "TestDevice0",
            }
        ];

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(homeMaticDeviceDetails));

        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(homeMaticDevices));

        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        // Act
        var devices = (await ccuClient.GetDevicesAsync()).ToArray();

        // Assert
        devices
            .Should()
            .HaveCount(2);

        devices[0]
            .Should()
            .Match<ICcuDevice>(x => x.Name == "TestDevice0");

        devices[1]
            .Should()
            .Match<ICcuDevice>(x => x.Name == string.Empty);
    }

    [Fact]
    public async Task GetDevicesAsync_HomeMaticIpDevicesAvailable_ReturnsCcuDevices()
    {
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        IEnumerable<DeviceDescription> homeMaticIpDevices =
        [
            new DeviceDescription
            {
                Address = "9876543210",
                Interface = "HmIP-RF"
            }
        ];

        IEnumerable<DeviceDetails> homeMaticIpDeviceDetails =
        [
            new DeviceDetails
            {
                Address = "9876543210",
                Interface = "HmIP-RF",
                Type = "HmIP-SWDO",
                Name = "TestDeviceIp",
            }
        ];

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(homeMaticIpDeviceDetails));

        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(homeMaticIpDevices));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        var devices = (await ccuClient.GetDevicesAsync()).ToArray();

        devices
            .Should()
            .HaveCount(1);

        devices[0]
            .Should()
            .Match<ICcuDevice>(x => x.Name == "TestDeviceIp");
    }

    [Fact]
    public async Task GetDevicesAsync_MixedDevicesAvailable_ReturnsAllCcuDevices()
    {
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        IEnumerable<DeviceDescription> homeMaticDevices =
        [
            new DeviceDescription
            {
                Address = "1234567890",
                Interface = "BidCos-RF"
            }
        ];

        IEnumerable<DeviceDescription> homeMaticIpDevices =
        [
            new DeviceDescription
            {
                Address = "9876543210",
                Interface = "HmIP-RF"
            }
        ];

        IEnumerable<DeviceDetails> deviceDetails =
        [
            new DeviceDetails
            {
                Address = "1234567890",
                Interface = "BidCos-RF",
                Type = "HmIP-SWDO",
                Name = "TestDevice0",
            },
            new DeviceDetails
            {
                Address = "9876543210",
                Interface = "HmIP-RF",
                Type = "HmIP-SWDO",
                Name = "TestDeviceIp",
            }
        ];

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(deviceDetails));

        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(homeMaticDevices));

        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(homeMaticIpDevices));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        var devices = (await ccuClient.GetDevicesAsync()).ToArray();

        devices
            .Should()
            .HaveCount(2);

        devices[0]
            .Should()
            .Match<ICcuDevice>(x => x.Name == "TestDevice0");

        devices[1]
            .Should()
            .Match<ICcuDevice>(x => x.Name == "TestDeviceIp");
    }

    [Fact]
    public async Task GetDevicesAsync_EmptyDeviceDetails_ReturnsDevicesWithEmptyNames()
    {
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        IEnumerable<DeviceDescription> homeMaticDevices =
        [
            new DeviceDescription
            {
                Address = "1234567890",
                Interface = "BidCos-RF"
            }
        ];

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));

        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(homeMaticDevices));

        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        var devices = (await ccuClient.GetDevicesAsync()).ToArray();

        devices
            .Should()
            .HaveCount(1);

        devices[0]
            .Should()
            .Match<ICcuDevice>(x => x.Name == string.Empty);
    }

    [Fact]
    public async Task GetDeviceAsync_UnknownAddress_ThrowsKeyNotFoundException()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        // Act
        var act = () => ccuClient.GetDeviceAsync("UNKNOWN");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Device with address 'UNKNOWN' not found.");
    }

    [Fact]
    public async Task GetDeviceAsync_MatchingAddressCaseInsensitive_ReturnsDevice()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult<IEnumerable<DeviceDescription>>(
            [
                new DeviceDescription { Address = "ABC1234", Interface = "BidCos-RF" }
            ]));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        // Act
        var device = await ccuClient.GetDeviceAsync("abc1234");

        // Assert
        device.Uri.Address.Should().Be("ABC1234");
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_DelegatesBuildingToCompleteCcuDeviceBuilder()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var completeBuilder = A.Fake<ICompleteCcuDeviceBuilder>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult<IEnumerable<DeviceDescription>>(
            [
                new DeviceDescription { Address = "A", Interface = "BidCos-RF" },
                new DeviceDescription { Address = "B", Interface = "BidCos-RF" }
            ]));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var completeDevice = A.Fake<ICompleteCcuDevice>();
        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>._))
            .Returns(Task.FromResult(completeDevice));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi, completeBuilder);

        // Act
        var devices = (await ccuClient.GetCompleteDevicesAsync()).ToList();

        // Assert
        devices.Should().HaveCount(2);
        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_NoDevices_ReturnsEmptyAndDoesNotCallBuilder()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var completeBuilder = A.Fake<ICompleteCcuDeviceBuilder>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi, completeBuilder);

        // Act
        var devices = await ccuClient.GetCompleteDevicesAsync();

        // Assert
        devices.Should().BeEmpty();
        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_ReturnsBuilderProducedDevices()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var completeBuilder = A.Fake<ICompleteCcuDeviceBuilder>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult<IEnumerable<DeviceDescription>>(
            [
                new DeviceDescription { Address = "A", Interface = "BidCos-RF" },
                new DeviceDescription { Address = "B", Interface = "BidCos-RF" }
            ]));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var completeDeviceA = A.Fake<ICompleteCcuDevice>();
        var completeDeviceB = A.Fake<ICompleteCcuDevice>();

        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>.That.Matches(d => d.Uri.Address == "A")))
            .Returns(Task.FromResult(completeDeviceA));
        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>.That.Matches(d => d.Uri.Address == "B")))
            .Returns(Task.FromResult(completeDeviceB));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi, completeBuilder);

        // Act
        var devices = (await ccuClient.GetCompleteDevicesAsync()).ToList();

        // Assert
        devices.Should().HaveCount(2);
        devices.Should().ContainInOrder(completeDeviceA, completeDeviceB);
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_KnownAddress_ReturnsBuilderResult()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var completeBuilder = A.Fake<ICompleteCcuDeviceBuilder>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult<IEnumerable<DeviceDescription>>(
            [
                new DeviceDescription { Address = "ABC1234", Interface = "BidCos-RF" }
            ]));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var expected = A.Fake<ICompleteCcuDevice>();
        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>.That.Matches(d => d.Uri.Address == "ABC1234")))
            .Returns(Task.FromResult(expected));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi, completeBuilder);

        // Act
        var device = await ccuClient.GetCompleteDeviceAsync("abc1234");

        // Assert
        device.Should().BeSameAs(expected);
        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_UnknownAddress_ThrowsKeyNotFoundException()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi);

        // Act
        var act = () => ccuClient.GetCompleteDeviceAsync("UNKNOWN");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_BuilderThrows_PropagatesException()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var completeBuilder = A.Fake<ICompleteCcuDeviceBuilder>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult<IEnumerable<DeviceDescription>>(
            [
                new DeviceDescription { Address = "A", Interface = "BidCos-RF" }
            ]));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>._))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi, completeBuilder);

        // Act
        var act = () => ccuClient.GetCompleteDevicesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_BuilderThrows_PropagatesException()
    {
        // Arrange
        var jsonRpcClient = A.Fake<IHomeMaticJsonRpcClient>();
        var homeMaticXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var homeMaticIpXmlRpcApi = A.Fake<IHomeMaticXmlRpcApi>();
        var completeBuilder = A.Fake<ICompleteCcuDeviceBuilder>();

        A.CallTo(() => jsonRpcClient.ListAllDetailsAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDetails>().AsEnumerable()));
        A.CallTo(() => homeMaticXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult<IEnumerable<DeviceDescription>>(
            [
                new DeviceDescription { Address = "ABC1234", Interface = "BidCos-RF" }
            ]));
        A.CallTo(() => homeMaticIpXmlRpcApi.ListDevicesAsync())
            .Returns(Task.FromResult(Array.Empty<DeviceDescription>().AsEnumerable()));

        A.CallTo(() => completeBuilder.BuildAsync(A<ICcuDevice>._))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var ccuClient = CreateCcuClient(jsonRpcClient, homeMaticXmlRpcApi, homeMaticIpXmlRpcApi, completeBuilder);

        // Act
        var act = () => ccuClient.GetCompleteDeviceAsync("ABC1234");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    private static CcuClient CreateCcuClient(IHomeMaticJsonRpcClient jsonRpcClient,
        IHomeMaticXmlRpcApi homeMaticXmlRpcApi,
        IHomeMaticXmlRpcApi homeMaticIpXmlRpcApi,
        ICompleteCcuDeviceBuilder? completeCcuDeviceBuilder = null)
    {
        var homeMaticXmlRpcApiConnection = new XmlRpcApiConnection(
            new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMatic),
            homeMaticXmlRpcApi);

        var homeMaticIpXmlRpcApiConnection = new XmlRpcApiConnection(
            new XmlRpcApiAddress(new Uri("http://example.com"), CcuDeviceKind.HomeMaticIp),
            homeMaticIpXmlRpcApi);

        var xmlRpcApis = new Dictionary<CcuDeviceKind, XmlRpcApiConnection>
        {
            { CcuDeviceKind.HomeMatic, homeMaticXmlRpcApiConnection },
            { CcuDeviceKind.HomeMaticIp, homeMaticIpXmlRpcApiConnection }
        };

        return new CcuClient(jsonRpcClient, xmlRpcApis,
            completeCcuDeviceBuilder ?? A.Fake<ICompleteCcuDeviceBuilder>());
    }
}
