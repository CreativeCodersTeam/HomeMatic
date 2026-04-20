using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuDeviceBaseTests
{
    private const string DeviceAddress = "BIDCOS:1";

    [Fact]
    public async Task GetParamSetValuesAsync_ReturnsMappedParamSetValuesFromApi()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var paramSet = new Dictionary<string, object>
        {
            ["TEMPERATURE"] = 21.5,
            ["HUMIDITY"] = 45
        };

        A.CallTo(() => api.GetParamSetAsync(DeviceAddress, "VALUES"))
            .Returns(Task.FromResult(paramSet));

        var device = CreateDevice(api);

        // Act
        var values = (await device.GetParamSetValuesAsync("VALUES")).ToList();

        // Assert
        values.Should().HaveCount(2);
        values.Should().Contain(v => v.Name == "TEMPERATURE" && Math.Abs((double)v.Value - 21.5) < 0.01);
        values.Should().Contain(v => v.Name == "HUMIDITY" && (int)v.Value == 45);
    }

    [Fact]
    public async Task GetParamSetValuesAsync_EmptyApiResult_ReturnsEmptyEnumerable()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetParamSetAsync(DeviceAddress, "MASTER"))
            .Returns(Task.FromResult(new Dictionary<string, object>()));

        var device = CreateDevice(api);

        // Act
        var values = await device.GetParamSetValuesAsync("MASTER");

        // Assert
        values.Should().BeEmpty();
    }

    [Fact]
    public async Task GetParamSetDescriptionsAsync_ReturnsMappedDescriptionsWithParamSetKey()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        var descriptions = new Dictionary<string, ParameterDescription>
        {
            ["TEMPERATURE"] = new()
            {
                Id = "TEMPERATURE",
                DefaultValue = 20.0,
                MinValue = 4.5,
                MaxValue = 30.5,
                Type = "FLOAT",
                DataType = ParameterDataType.Float,
                Unit = "°C",
                TabOrder = 1,
                Control = "THERMOSTAT.SET_TEMPERATURE",
                ValuesList = [],
                SpecialValues = []
            }
        };

        A.CallTo(() => api.GetParameterDescriptionAsync(DeviceAddress, "VALUES"))
            .Returns(Task.FromResult(descriptions));

        var device = CreateDevice(api);

        // Act
        var result = await device.GetParamSetDescriptionsAsync("VALUES");

        // Assert
        result.ParamSetKey.Should().Be("VALUES");
        var items = result.Items.ToList();
        items.Should().HaveCount(1);

        var item = items[0];
        item.Id.Should().Be("TEMPERATURE");
        item.DefaultValue.Should().Be(20.0);
        item.MinValue.Should().Be(4.5);
        item.MaxValue.Should().Be(30.5);
        item.Type.Should().Be("FLOAT");
        item.DataType.Should().Be(ParameterDataType.Float);
        item.Unit.Should().Be("°C");
        item.TabOrder.Should().Be(1);
        item.Control.Should().Be("THERMOSTAT.SET_TEMPERATURE");
    }

    [Fact]
    public async Task GetParamSetDescriptionsAsync_EmptyApiResult_ReturnsResultWithNoItems()
    {
        // Arrange
        var api = A.Fake<IHomeMaticXmlRpcApi>();
        A.CallTo(() => api.GetParameterDescriptionAsync(DeviceAddress, "MASTER"))
            .Returns(Task.FromResult(new Dictionary<string, ParameterDescription>()));

        var device = CreateDevice(api);

        // Act
        var result = await device.GetParamSetDescriptionsAsync("MASTER");

        // Assert
        result.ParamSetKey.Should().Be("MASTER");
        result.Items.Should().BeEmpty();
    }

    private static CcuDevice CreateDevice(IHomeMaticXmlRpcApi api)
    {
        return new CcuDevice(api)
        {
            Uri = new CcuDeviceUri
            {
                CcuHost = "localhost",
                Kind = CcuDeviceKind.HomeMatic,
                Address = DeviceAddress
            },
            DeviceType = "TestType",
            IsAesActive = false,
            Interface = "BidCos-RF",
            Version = 1,
            Roaming = false,
            ParamSets = ["MASTER", "VALUES"],
            RxMode = RxModes.Always,
            RfAddress = 0,
            Firmware = "1.0.0",
            AvailableFirmware = "1.0.0",
            CanBeUpdated = false,
            FirmwareUpdateState = DeviceFirmwareUpdateState.None,
            Channels = []
        };
    }
}
