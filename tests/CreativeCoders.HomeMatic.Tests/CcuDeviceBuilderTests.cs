using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using FakeItEasy;
using FluentAssertions;

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
            RxMode = RxMode.Always,
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
            Host = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "1234567890"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri)
            .WithApi(api)
            .FromDeviceDescription(deviceDescription);

        // Act
        var ccuDevice = builder.Build();

        // Assert
        ccuDevice.Uri.Should().Be(uri);
        ccuDevice.DeviceType.Should().Be("TestType");
        ccuDevice.Version.Should().Be(1);
        ccuDevice.Index.Should().Be(2);
        ccuDevice.IsAesActive.Should().BeTrue();
        ccuDevice.Interface.Should().Be("TestInterface");
        ccuDevice.RxMode.Should().Be(RxMode.Always);
        ccuDevice.Group.Should().Be("TestGroup");
        ccuDevice.RfAddress.Should().Be(12345);
        ccuDevice.Firmware.Should().Be("1.0.0");
        ccuDevice.AvailableFirmware.Should().Be("1.1.0");
        ccuDevice.CanBeUpdated.Should().BeTrue();
        ccuDevice.FirmwareUpdateState.Should().Be(DeviceFirmwareUpdateState.NewFirmwareAvailable);
        ccuDevice.Roaming.Should().BeTrue();
        ccuDevice.ChannelDirection.Should().Be(ChannelDirection.Receiver);
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
            .WithMessage("Uri and Api must be set");
    }

    [Fact]
    public void Build_WithoutApi_ThrowsInvalidOperationException()
    {
        // Arrange
        var uri = new CcuDeviceUri
        {
            Host = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "1234567890"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri);

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Uri and Api must be set");
    }

    [Fact]
    public void Build_WithNullDeviceDescription_ReturnsCcuDeviceWithDefaultProperties()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var uri = new CcuDeviceUri
        {
            Host = "localhost",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "1234567890"
        };

        var builder = new CcuDeviceBuilder()
            .WithUri(uri)
            .WithApi(api);

        // Act
        var ccuDevice = builder.Build();

        // Assert
        ccuDevice.DeviceType.Should().Be(string.Empty);
        ccuDevice.Version.Should().Be(0);
        ccuDevice.Index.Should().Be(0);
        ccuDevice.IsAesActive.Should().BeFalse();
        ccuDevice.Interface.Should().Be(string.Empty);
        ccuDevice.RxMode.Should().Be(RxMode.None);
        ccuDevice.Group.Should().Be(string.Empty);
        ccuDevice.RfAddress.Should().Be(0);
        ccuDevice.Firmware.Should().Be(string.Empty);
        ccuDevice.AvailableFirmware.Should().Be(string.Empty);
        ccuDevice.CanBeUpdated.Should().BeFalse();
        ccuDevice.FirmwareUpdateState.Should().Be(DeviceFirmwareUpdateState.None);
        ccuDevice.Roaming.Should().BeFalse();
        ccuDevice.ChannelDirection.Should().Be(ChannelDirection.None);
        ccuDevice.ParamSets.Should().BeEmpty();
    }
}
