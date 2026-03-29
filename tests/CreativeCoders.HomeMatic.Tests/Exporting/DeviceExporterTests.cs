using System.Text.Json;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.Exporting;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests.Exporting;

public class DeviceExporterTests
{
    private static CcuDeviceUri CreateDeviceUri(string host = "ccu2.local", string address = "ABC123456")
    {
        return new CcuDeviceUri
        {
            CcuHost = host,
            Kind = CcuDeviceKind.HomeMatic,
            Address = address
        };
    }

    /// <summary>
    /// Parameter object for configuring a fake <see cref="ICompleteCcuDevice"/> in tests.
    /// </summary>
    private sealed class FakeDeviceOptions
    {
        public string Name { get; init; } = "TestDevice";
        public string Address { get; init; } = "ABC123456";
        public string DeviceType { get; init; } = "HM-CC-RT-DN";
        public string Firmware { get; init; } = "1.4";
        public string CcuHost { get; init; } = "ccu2.local";
        public string[]? ParamSetKeys { get; init; }
        public IEnumerable<ParamSetValuesWithDescriptions>? ParamSetValues { get; init; }
        public IEnumerable<ICompleteCcuDeviceChannel>? Channels { get; init; }
    }

    private static ICompleteCcuDevice CreateFakeDevice(FakeDeviceOptions? options = null)
    {
        var opts = options ?? new FakeDeviceOptions();
        var device = A.Fake<ICompleteCcuDevice>();
        var deviceData = A.Fake<ICcuDeviceData>();
        var uri = CreateDeviceUri(opts.CcuHost, opts.Address);

        A.CallTo(() => deviceData.Name).Returns(opts.Name);
        A.CallTo(() => deviceData.Uri).Returns(uri);
        A.CallTo(() => deviceData.DeviceType).Returns(opts.DeviceType);
        A.CallTo(() => deviceData.Firmware).Returns(opts.Firmware);
        A.CallTo(() => deviceData.ParamSets).Returns(opts.ParamSetKeys ?? ["MASTER", "VALUES"]);

        A.CallTo(() => device.DeviceData).Returns(deviceData);
        A.CallTo(() => device.ParamSetValues).Returns(opts.ParamSetValues ?? []);
        A.CallTo(() => device.Channels).Returns(opts.Channels ?? []);

        return device;
    }

    private static ICompleteCcuDeviceChannel CreateFakeChannel(
        string address = "ABC123456:1",
        string deviceType = "HM-CC-RT-DN:01",
        int index = 1,
        string[]? paramSets = null,
        IEnumerable<ParamSetValuesWithDescriptions>? paramSetValues = null)
    {
        var channel = A.Fake<ICompleteCcuDeviceChannel>();
        var channelData = A.Fake<ICcuDeviceChannelData>();
        var uri = new CcuDeviceUri
        {
            CcuHost = "ccu2.local",
            Kind = CcuDeviceKind.HomeMatic,
            Address = address
        };

        A.CallTo(() => channelData.Uri).Returns(uri);
        A.CallTo(() => channelData.DeviceType).Returns(deviceType);
        A.CallTo(() => channelData.Index).Returns(index);
        A.CallTo(() => channelData.ParamSets).Returns(paramSets ?? ["VALUES"]);

        A.CallTo(() => channel.ChannelData).Returns(channelData);
        A.CallTo(() => channel.ParamSetValues).Returns(paramSetValues ?? []);

        return channel;
    }

    private static ParamSetValuesWithDescriptions CreateParamSetValues(
        string paramSetKey,
        params (string name, object value, string descriptionId)[] values)
    {
        return new ParamSetValuesWithDescriptions
        {
            ParamSetKey = paramSetKey,
            ParamSetValues = values.Select(v => new ParamSetValueWithDescription
            {
                ParamSetValue = new ParamSetValue { Name = v.name, Value = v.value },
                Description = new CcuParameterDescription
                {
                    Id = v.descriptionId,
                    DefaultValue = null,
                    MinValue = null,
                    MaxValue = null,
                    Type = null,
                    DataType = ParameterDataType.Float,
                    Unit = null,
                    TabOrder = 0,
                    Control = null,
                    ValuesList = [],
                    SpecialValues = []
                }
            }).ToList()
        };
    }

    [Fact]
    public void BuildExportData_WithValidDevice_MapsNameCorrectly()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Name = "MyDevice" });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Name.Should().Be("MyDevice");
    }

    [Fact]
    public void BuildExportData_WithValidDevice_MapsAddressCorrectly()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Address = "DEF789" });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Address.Should().Be("DEF789");
    }

    [Fact]
    public void BuildExportData_WithValidDevice_MapsDeviceTypeCorrectly()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { DeviceType = "HM-ES-PMSw1-Pl" });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.DeviceType.Should().Be("HM-ES-PMSw1-Pl");
    }

    [Fact]
    public void BuildExportData_WithValidDevice_MapsFirmwareVersionCorrectly()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Firmware = "2.7.1" });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.FirmwareVersion.Should().Be("2.7.1");
    }

    [Fact]
    public void BuildExportData_WithCcuHostName_UsesCcuNameAsHostDisplayName()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { CcuHost = "192.168.1.100" });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Ccu.Should().Be("192.168.1.100");
    }

    [Fact]
    public void BuildExportData_WithParamSetKeys_MapsParamSetKeysCorrectly()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetKeys = ["MASTER", "VALUES", "LINK"] });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetKeys.Should().BeEquivalentTo("MASTER", "VALUES", "LINK");
    }

    [Fact]
    public void BuildExportData_WithNoChannels_ReturnsEmptyChannelsList()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Channels = [] });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Channels.Should().BeEmpty();
    }

    [Fact]
    public void BuildExportData_WithChannels_MapsChannelsCorrectly()
    {
        // Arrange
        var channel = CreateFakeChannel(address: "XYZ:1", deviceType: "HM-CC-RT-DN:01", index: 1);
        var device = CreateFakeDevice(new FakeDeviceOptions { Channels = [channel] });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Channels.Should().HaveCount(1);
        var channelResult = result.Channels.First();
        channelResult.Address.Should().Be("XYZ:1");
        channelResult.DeviceType.Should().Be("HM-CC-RT-DN:01");
        channelResult.Index.Should().Be(1);
    }

    [Fact]
    public void BuildExportData_WithParamSetValues_MapsParamSetValuesCorrectly()
    {
        // Arrange
        var paramSetValues = new[]
        {
            CreateParamSetValues("MASTER", ("BOOST_TIME", 5, "Boost Time"))
        };
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetValues = paramSetValues });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Should().HaveCount(1);
        var paramSet = result.ParamSetValues.First();
        paramSet.ParamSetKey.Should().Be("MASTER");
        paramSet.Values.Should().HaveCount(1);
        paramSet.Values.First().Key.Should().Be("BOOST_TIME");
        paramSet.Values.First().Value.Should().Be(5);
        paramSet.Values.First().Name.Should().Be("Boost Time");
    }

    [Fact]
    public void BuildExportData_WithOptionsWhitelistingParamSet_FiltersOutNonWhitelistedParamSets()
    {
        // Arrange
        var paramSetValues = new[]
        {
            CreateParamSetValues("MASTER", ("BOOST_TIME", 5, "Boost Time")),
            CreateParamSetValues("LINK", ("PEER_NEEDS_BURST", true, "Peer Needs Burst"))
        };
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetValues = paramSetValues });
        var options = new DeviceExportOptions { ParamSetWhitelist = ["MASTER"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.ParamSetValues.Should().HaveCount(1);
        result.ParamSetValues.First().ParamSetKey.Should().Be("MASTER");
    }

    [Fact]
    public void BuildExportData_WithNullOptions_IncludesAllParamSets()
    {
        // Arrange
        var paramSetValues = new[]
        {
            CreateParamSetValues("MASTER", ("BOOST_TIME", 5, "Boost Time")),
            CreateParamSetValues("VALUES", ("SET_TEMPERATURE", 21.0, "Set Temperature"))
        };
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetValues = paramSetValues });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, null);

        // Assert
        result.ParamSetValues.Should().HaveCount(2);
    }

    [Fact]
    public void BuildExportData_WithChannelParamSetValues_MapsChannelParamSetValuesCorrectly()
    {
        // Arrange
        var channelParamSetValues = new[]
        {
            CreateParamSetValues("VALUES", ("SET_TEMPERATURE", 21.5, "Set Temperature"))
        };
        var channel = CreateFakeChannel(paramSetValues: channelParamSetValues);
        var device = CreateFakeDevice(new FakeDeviceOptions { Channels = [channel] });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        var channelResult = result.Channels.First();
        channelResult.ParamSetValues.Should().HaveCount(1);
        channelResult.ParamSetValues.First().ParamSetKey.Should().Be("VALUES");
        channelResult.ParamSetValues.First().Values.First().Key.Should().Be("SET_TEMPERATURE");
    }

    [Fact]
    public async Task ExportDeviceAsync_WithValidDevice_ReturnsValidJson()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Name = "TestDevice" });
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);

        // Assert
        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExportDeviceAsync_WithValidDevice_ContainsDeviceName()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Name = "MyThermostat" });
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);

        // Assert
        json.Should().Contain("MyThermostat");
    }

    [Fact]
    public async Task ExportDeviceAsync_WithWriteIndentedTrue_ReturnsIndentedJson()
    {
        // Arrange
        var device = CreateFakeDevice();
        var options = new DeviceExportOptions { WriteIndented = true };
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device, options);

        // Assert
        json.Should().Contain("\n");
    }

    [Fact]
    public async Task ExportDeviceAsync_WithWriteIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var device = CreateFakeDevice();
        var options = new DeviceExportOptions { WriteIndented = false };
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device, options);

        // Assert
        json.Should().NotContain("\n");
    }

    [Fact]
    public async Task ExportDevicesAsync_WithMultipleDevices_ReturnsJsonArray()
    {
        // Arrange
        var device1 = CreateFakeDevice(new FakeDeviceOptions { Name = "Device1", Address = "ADDR1" });
        var device2 = CreateFakeDevice(new FakeDeviceOptions { Name = "Device2", Address = "ADDR2" });
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([device1, device2]);

        // Assert
        var document = JsonDocument.Parse(json);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        document.RootElement.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task ExportDevicesAsync_WithEmptyList_ReturnsEmptyJsonArray()
    {
        // Arrange
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([]);

        // Assert
        var document = JsonDocument.Parse(json);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        document.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task ExportDevicesAsync_WithOptions_FiltersParamSetsPerDevice()
    {
        // Arrange
        var paramSetValues = new[]
        {
            CreateParamSetValues("MASTER", ("BOOST_TIME", 5, "Boost Time")),
            CreateParamSetValues("LINK", ("PEER_NEEDS_BURST", true, "Peer Needs Burst"))
        };
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetValues = paramSetValues });
        var options = new DeviceExportOptions { ParamSetWhitelist = ["MASTER"] };
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([device], options);

        // Assert
        json.Should().Contain("MASTER");
        json.Should().NotContain("LINK");
    }

    [Fact]
    public async Task ExportDeviceAsync_WithCamelCasePolicy_UsesCamelCasePropertyNames()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Name = "TestDevice" });
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);

        // Assert
        json.Should().Contain("\"name\"");
        json.Should().Contain("\"address\"");
        json.Should().Contain("\"deviceType\"");
        json.Should().NotContain("\"Name\"");
        json.Should().NotContain("\"Address\"");
    }

    [Fact]
    public void BuildExportData_WithMultipleChannels_MapsAllChannels()
    {
        // Arrange
        var channel1 = CreateFakeChannel(address: "XYZ:1", index: 1);
        var channel2 = CreateFakeChannel(address: "XYZ:2", index: 2);
        var channel3 = CreateFakeChannel(address: "XYZ:3", index: 3);
        var device = CreateFakeDevice(new FakeDeviceOptions { Channels = [channel1, channel2, channel3] });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Channels.Should().HaveCount(3);
        result.Channels.Select(c => c.Index).Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void BuildExportData_WithChannelOptionsWhitelist_FiltersChannelParamSets()
    {
        // Arrange
        var channelParamSetValues = new[]
        {
            CreateParamSetValues("VALUES", ("SET_TEMPERATURE", 21.5, "Set Temperature")),
            CreateParamSetValues("LINK", ("PEER_NEEDS_BURST", true, "Peer Needs Burst"))
        };
        var channel = CreateFakeChannel(paramSetValues: channelParamSetValues);
        var device = CreateFakeDevice(new FakeDeviceOptions { Channels = [channel] });
        var options = new DeviceExportOptions { ParamSetWhitelist = ["VALUES"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.Channels.First().ParamSetValues.Should().HaveCount(1);
        result.Channels.First().ParamSetValues.First().ParamSetKey.Should().Be("VALUES");
    }

    [Fact]
    public void BuildExportData_WithChannelOptionsWhitelistCaseInsensitive_FiltersCorrectly()
    {
        // Arrange
        var channelParamSetValues = new[]
        {
            CreateParamSetValues("VALUES", ("SET_TEMPERATURE", 21.5, "Set Temperature")),
            CreateParamSetValues("LINK", ("PEER_NEEDS_BURST", true, "Peer Needs Burst"))
        };
        var channel = CreateFakeChannel(paramSetValues: channelParamSetValues);
        var device = CreateFakeDevice(new FakeDeviceOptions { Channels = [channel] });
        // Use lowercase key in whitelist to test case-insensitive matching
        var options = new DeviceExportOptions { ParamSetWhitelist = ["values"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.Channels.First().ParamSetValues.Should().HaveCount(1);
        result.Channels.First().ParamSetValues.First().ParamSetKey.Should().Be("VALUES");
    }

    [Fact]
    public void BuildExportData_WithEmptyParamSetValues_ReturnsEmptyParamSetValuesList()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetValues = [] });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportDeviceAsync_WithNullParamValueName_OmitsNamePropertyFromJson()
    {
        // Arrange
        var paramSetValues = new[]
        {
            new ParamSetValuesWithDescriptions
            {
                ParamSetKey = "VALUES",
                ParamSetValues =
                [
                    new ParamSetValueWithDescription
                    {
                        ParamSetValue = new ParamSetValue { Name = "ACTIVE", Value = true },
                        Description = new CcuParameterDescription
                        {
                            Id = null, // null description id → Name will be null
                            DefaultValue = null,
                            MinValue = null,
                            MaxValue = null,
                            Type = null,
                            DataType = ParameterDataType.Bool,
                            Unit = null,
                            TabOrder = 0,
                            Control = null,
                            ValuesList = [],
                            SpecialValues = []
                        }
                    }
                ]
            }
        };
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetValues = paramSetValues });
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);

        // Assert
        // DefaultIgnoreCondition.WhenWritingNull should omit null name
        json.Should().NotContain("\"name\":null");
    }

    [Fact]
    public async Task ExportDevicesAsync_WithSingleDevice_ReturnsArrayWithOneElement()
    {
        // Arrange
        var device = CreateFakeDevice(new FakeDeviceOptions { Name = "OnlyDevice" });
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([device]);

        // Assert
        var document = JsonDocument.Parse(json);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        document.RootElement.GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task ExportDeviceAsync_WithCcuNameSet_UsesCcuNameInOutput()
    {
        // Arrange
        var device = A.Fake<ICompleteCcuDevice>();
        var deviceData = A.Fake<ICcuDeviceData>();
        var uri = new CcuDeviceUri
        {
            CcuHost = "192.168.1.100",
            CcuName = "MyCCU2",
            Kind = CcuDeviceKind.HomeMatic,
            Address = "ADDR1"
        };
        A.CallTo(() => deviceData.Name).Returns("Device");
        A.CallTo(() => deviceData.Uri).Returns(uri);
        A.CallTo(() => deviceData.DeviceType).Returns("HM-CC-RT-DN");
        A.CallTo(() => deviceData.Firmware).Returns("1.0");
        A.CallTo(() => deviceData.ParamSets).Returns([]);
        A.CallTo(() => device.DeviceData).Returns(deviceData);
        A.CallTo(() => device.ParamSetValues).Returns([]);
        A.CallTo(() => device.Channels).Returns([]);
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        // HostDisplayName returns CcuName when set
        result.Ccu.Should().Be("MyCCU2");
    }

    [Fact]
    public void BuildExportData_WithEmptyParamSetInParamSetValues_ReturnsParamSetWithEmptyValues()
    {
        // Arrange
        var paramSetValues = new[]
        {
            new ParamSetValuesWithDescriptions
            {
                ParamSetKey = "MASTER",
                ParamSetValues = []
            }
        };
        var device = CreateFakeDevice(new FakeDeviceOptions { ParamSetValues = paramSetValues });
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Should().HaveCount(1);
        result.ParamSetValues.First().Values.Should().BeEmpty();
    }
}
