using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuDeviceBuilderTests
{
    [Fact]
    public void Build_WithValidUriAndApi_ReturnsCcuDeviceWithCorrectProperties()
    {
        // Arrange
        var deviceDescription = new DeviceDescription
        {
            DeviceType = "TestType",
            Version = 1,
            Index = 2,
            IsAesActive = true,
            Interface = "TestInterface",
            RxMode = RxModes.Always,
            Group = "TestGroup",
            RfAddress = 12345,
            Firmware = "1.0.0",
            AvailableFirmware = "1.1.0",
            CanBeUpdated = true,
            FirmwareUpdateState = DeviceFirmwareUpdateState.NewFirmwareAvailable,
            Roaming = true,
            ChannelDirection = ChannelDirection.Receiver,
            ParamSets = ["ParamSet1", "ParamSet2"]
        };

        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var uri = new CcuDeviceUri
        {
            CcuHost = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "1234567890"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri)
            .WithApi(api)
            .WithAllDevices([])
            .FromDeviceDescription(deviceDescription);

        // Act
        var ccuDevice = builder.Build();

        // Assert
        ccuDevice.Uri.Should().Be(uri);
        ccuDevice.DeviceType.Should().Be("TestType");
        ccuDevice.Version.Should().Be(1);
        ccuDevice.IsAesActive.Should().BeTrue();
        ccuDevice.Interface.Should().Be("TestInterface");
        ccuDevice.RxMode.Should().Be(RxModes.Always);
        ccuDevice.RfAddress.Should().Be(12345);
        ccuDevice.Firmware.Should().Be("1.0.0");
        ccuDevice.AvailableFirmware.Should().Be("1.1.0");
        ccuDevice.CanBeUpdated.Should().BeTrue();
        ccuDevice.FirmwareUpdateState.Should().Be(DeviceFirmwareUpdateState.NewFirmwareAvailable);
        ccuDevice.Roaming.Should().BeTrue();
        ccuDevice.ParamSets.Should().HaveCount(2);
        ccuDevice.ParamSets.Should().Contain("ParamSet1");
        ccuDevice.ParamSets.Should().Contain("ParamSet2");
    }

    [Fact]
    public void Build_WithoutUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();

        var builder = new CcuDeviceBuilder()
            .WithApi(api);

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Uri, Api and Devices must be set");
    }

    [Fact]
    public void Build_WithoutApi_ThrowsInvalidOperationException()
    {
        // Arrange
        var uri = new CcuDeviceUri
        {
            CcuHost = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "1234567890"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri);

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Uri, Api and Devices must be set");
    }

    [Fact]
    public void Build_WithoutDevices_ThrowsInvalidOperationException()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var uri = new CcuDeviceUri
        {
            CcuHost = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "1234567890"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri)
            .WithApi(api);

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Uri, Api and Devices must be set");
    }

    [Fact]
    public void Build_WithDeviceDescriptionAndChildDevices_CreatesChannelsOrderedByIndex()
    {
        // Arrange - parent device has two children (out of order) and an unrelated device.
        const string parentAddress = "PARENT1";

        var parent = new DeviceDescription
        {
            Address = parentAddress,
            Parent = string.Empty,
            DeviceType = "ParentType",
            ParamSets = ["MASTER"]
        };

        var channel2 = new DeviceDescription
        {
            Address = parentAddress + ":2",
            Parent = parentAddress,
            DeviceType = "ChannelType",
            Index = 2,
            Group = "G2",
            ChannelDirection = ChannelDirection.Sender,
            Interface = "BidCos-RF",
            Version = 7,
            IsAesActive = true,
            Roaming = false,
            ParamSets = ["VALUES"]
        };

        var channel1 = new DeviceDescription
        {
            Address = parentAddress + ":1",
            Parent = parentAddress,
            DeviceType = "ChannelType",
            Index = 1,
            Group = "G1",
            ChannelDirection = ChannelDirection.Receiver,
            Interface = "BidCos-RF",
            Version = 7,
            IsAesActive = false,
            Roaming = false,
            ParamSets = ["VALUES"]
        };

        var unrelated = new DeviceDescription
        {
            Address = "OTHER:1",
            Parent = "OTHER",
            DeviceType = "ChannelType",
            Index = 1
        };

        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var uri = new CcuDeviceUri
        {
            CcuHost = "localhost",
            CcuName = "ccu",
            Kind = CcuDeviceKind.HomeMatic,
            Address = parentAddress
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri)
            .WithApi(api)
            .WithAllDevices([parent, channel2, channel1, unrelated])
            .FromDeviceDescription(parent);

        // Act
        var ccuDevice = builder.Build();

        // Assert - only child channels are created and they are ordered by Index.
        var channels = ccuDevice.Channels.ToList();
        channels.Should().HaveCount(2);
        channels[0].Uri.Address.Should().Be(parentAddress + ":1");
        channels[0].Index.Should().Be(1);
        channels[0].ChannelDirection.Should().Be(ChannelDirection.Receiver);
        channels[1].Uri.Address.Should().Be(parentAddress + ":2");
        channels[1].Index.Should().Be(2);
        channels[1].ChannelDirection.Should().Be(ChannelDirection.Sender);
        channels.Should().AllSatisfy(c => c.Uri.CcuHost.Should().Be("localhost"));
        channels.Should().AllSatisfy(c => c.Uri.CcuName.Should().Be("ccu"));
        channels.Should().AllSatisfy(c => c.Uri.Kind.Should().Be(CcuDeviceKind.HomeMatic));
    }

    [Fact]
    public void Build_WithNullDeviceDescription_ReturnsCcuDeviceWithDefaultProperties()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var uri = new CcuDeviceUri
        {
            CcuHost = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "1234567890"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri)
            .WithApi(api)
            .WithAllDevices([]);

        // Act
        var ccuDevice = builder.Build();

        // Assert
        ccuDevice.DeviceType.Should().Be(string.Empty);
        ccuDevice.Version.Should().Be(0);
        ccuDevice.IsAesActive.Should().BeFalse();
        ccuDevice.Interface.Should().Be(string.Empty);
        ccuDevice.RxMode.Should().Be(RxModes.None);
        ccuDevice.RfAddress.Should().Be(0);
        ccuDevice.Firmware.Should().Be(string.Empty);
        ccuDevice.AvailableFirmware.Should().Be(string.Empty);
        ccuDevice.CanBeUpdated.Should().BeFalse();
        ccuDevice.FirmwareUpdateState.Should().Be(DeviceFirmwareUpdateState.None);
        ccuDevice.Roaming.Should().BeFalse();
        ccuDevice.ParamSets.Should().BeEmpty();
    }

    [Fact]
    public void Build_WithDeviceDescription_UsesCaseInsensitiveParentMatchingForChannels()
    {
        // Arrange - parent uses upper case, child references lower case parent address.
        var parent = new DeviceDescription
        {
            Address = "PARENT1",
            Parent = string.Empty,
            DeviceType = "ParentType",
            ParamSets = []
        };

        var child = new DeviceDescription
        {
            Address = "PARENT1:1",
            Parent = "parent1",
            DeviceType = "ChannelType",
            Index = 1,
            Group = string.Empty,
            ChannelDirection = ChannelDirection.Receiver,
            Interface = "BidCos-RF",
            Version = 1,
            IsAesActive = false,
            Roaming = false,
            ParamSets = []
        };

        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var uri = new CcuDeviceUri
        {
            CcuHost = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "PARENT1"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri)
            .WithApi(api)
            .WithAllDevices([parent, child])
            .FromDeviceDescription(parent);

        // Act
        var ccuDevice = builder.Build();

        // Assert
        ccuDevice.Channels.Should().ContainSingle()
            .Which.Uri.Address.Should().Be("PARENT1:1");
    }
}
