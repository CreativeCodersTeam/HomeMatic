using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuDeviceTests
{
    [Fact]
    public async Task GetChannelAsync_WithMatchingAddress_ReturnsChannel()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var channel1 = CreateChannel(api, "DEV:1", 1);
        var channel2 = CreateChannel(api, "DEV:2", 2);

        var device = CreateDevice(api, [channel1, channel2]);

        // Act
        var result = await device.GetChannelAsync("DEV:2");

        // Assert
        result.Should().BeSameAs(channel2);
    }

    [Fact]
    public async Task GetChannelAsync_WithUnknownAddress_ThrowsKeyNotFoundException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var device = CreateDevice(api, [CreateChannel(api, "DEV:1", 1)]);

        // Act
        var act = () => device.GetChannelAsync("DEV:UNKNOWN");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Channel with address 'DEV:UNKNOWN' not found.");
    }

    [Fact]
    public async Task GetChannelAsync_WithNoChannels_ThrowsKeyNotFoundException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var device = CreateDevice(api, []);

        // Act
        var act = () => device.GetChannelAsync("DEV:1");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static CcuDevice CreateDevice(IHomeMaticXmlRpcApi api, IEnumerable<ICcuDeviceChannel> channels)
    {
        return new CcuDevice(api)
        {
            Uri = new CcuDeviceUri
            {
                CcuHost = "localhost",
                Kind = CcuDeviceKind.HomeMatic,
                Address = "DEV"
            },
            DeviceType = "TestType",
            IsAesActive = false,
            Interface = "BidCos-RF",
            Version = 1,
            Roaming = false,
            ParamSets = [],
            RxMode = RxModes.Always,
            RfAddress = 0,
            Firmware = "1.0.0",
            AvailableFirmware = "1.0.0",
            CanBeUpdated = false,
            FirmwareUpdateState = DeviceFirmwareUpdateState.None,
            Channels = channels
        };
    }

    private static CcuDeviceChannel CreateChannel(IHomeMaticXmlRpcApi api, string address, int index)
    {
        return new CcuDeviceChannel(api)
        {
            Uri = new CcuDeviceUri
            {
                CcuHost = "localhost",
                Kind = CcuDeviceKind.HomeMatic,
                Address = address
            },
            DeviceType = "ChannelType",
            IsAesActive = false,
            Interface = "BidCos-RF",
            Version = 1,
            Roaming = false,
            ParamSets = [],
            Index = index,
            Group = string.Empty,
            ChannelDirection = ChannelDirection.Receiver
        };
    }
}
