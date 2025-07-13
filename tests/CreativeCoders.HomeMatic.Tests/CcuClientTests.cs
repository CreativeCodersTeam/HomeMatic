using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using FakeItEasy;
using FluentAssertions;

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

    private static CcuClient CreateCcuClient(IHomeMaticJsonRpcClient jsonRpcClient,
        IHomeMaticXmlRpcApi homeMaticXmlRpcApi,
        IHomeMaticXmlRpcApi homeMaticIpXmlRpcApi)
    {
        var homeMaticXmlRpcApiConnection = new XmlRpcApiConnection(
            new XmlRpcEndpoint(new Uri("http://example.com"), CcuDeviceKind.HomeMatic),
            homeMaticXmlRpcApi);

        var homeMaticIpXmlRpcApiConnection = new XmlRpcApiConnection(
            new XmlRpcEndpoint(new Uri("http://example.com"), CcuDeviceKind.HomeMaticIp),
            homeMaticIpXmlRpcApi);

        var xmlRpcApis = new Dictionary<CcuDeviceKind, XmlRpcApiConnection>
        {
            { CcuDeviceKind.HomeMatic, homeMaticXmlRpcApiConnection },
            { CcuDeviceKind.HomeMaticIp, homeMaticIpXmlRpcApiConnection }
        };

        return new CcuClient(jsonRpcClient, xmlRpcApis);
    }
}
