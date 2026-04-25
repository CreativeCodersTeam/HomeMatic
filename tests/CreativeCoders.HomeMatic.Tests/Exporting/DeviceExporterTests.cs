using System.Text.Json;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.Exporting;

namespace CreativeCoders.HomeMatic.Tests.Exporting;

public class DeviceExporterTests
{
    // ---- Mapping: BuildExportData -----------------------------------------

    [Fact]
    public void BuildExportData_WithFullyPopulatedDevice_MapsAllScalarProperties()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithName("MyDevice")
            .WithAddress("DEF789")
            .WithDeviceType("HM-ES-PMSw1-Pl")
            .WithFirmware("2.7.1")
            .WithCcuHost("192.168.1.100")
            .WithParamSetKeys("MASTER", "VALUES", "LINK")
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Name.Should().Be("MyDevice");
        result.Address.Should().Be("DEF789");
        result.DeviceType.Should().Be("HM-ES-PMSw1-Pl");
        result.FirmwareVersion.Should().Be("2.7.1");
        result.Ccu.Should().Be("192.168.1.100");
        result.ParamSetKeys.Should().BeEquivalentTo("MASTER", "VALUES", "LINK");
    }

    [Fact]
    public void BuildExportData_WithCcuNameSet_UsesCcuNameForCcu()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithCcuHost("192.168.1.100")
            .WithCcuName("MyCCU2")
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Ccu.Should().Be("MyCCU2");
    }

    [Fact]
    public void BuildExportData_WithCcuNameEmpty_FallsBackToCcuHost()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithCcuHost("ccu2.local")
            .WithCcuName(string.Empty)
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Ccu.Should().Be("ccu2.local");
    }

    [Fact]
    public void BuildExportData_WithNoChannels_ReturnsEmptyChannelsList()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder().Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Channels.Should().BeEmpty();
    }

    [Fact]
    public void BuildExportData_WithMultipleChannels_MapsAllInOrder()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithChannel(c => c.WithAddress("XYZ:1").WithIndex(1))
            .WithChannel(c => c.WithAddress("XYZ:2").WithIndex(2))
            .WithChannel(c => c.WithAddress("XYZ:3").WithIndex(3))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.Channels.Should().HaveCount(3);
        result.Channels.Select(c => c.Index).Should().ContainInOrder(1, 2, 3);
        result.Channels.Select(c => c.Address).Should().ContainInOrder("XYZ:1", "XYZ:2", "XYZ:3");
    }

    [Fact]
    public void BuildExportData_WithChannel_MapsChannelScalarProperties()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithChannel(c => c
                .WithAddress("XYZ:1")
                .WithDeviceType("HM-CC-RT-DN:01")
                .WithIndex(1)
                .WithParamSets("VALUES", "MASTER"))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        var channel = result.Channels.Single();
        channel.Address.Should().Be("XYZ:1");
        channel.DeviceType.Should().Be("HM-CC-RT-DN:01");
        channel.Index.Should().Be(1);
        channel.ParamSets.Should().BeEquivalentTo("VALUES", "MASTER");
    }

    [Fact]
    public void BuildExportData_WithNoParamSetValues_ReturnsEmptyList()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder().Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Should().BeEmpty();
    }

    [Fact]
    public void BuildExportData_WithParamSetValues_MapsKeyAndValues()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p.Add("BOOST_TIME", 5, "Boost Time"))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        var paramSet = result.ParamSetValues.Single();
        paramSet.ParamSetKey.Should().Be("MASTER");

        var value = paramSet.Values.Single();
        value.Key.Should().Be("BOOST_TIME");
        value.Value.Should().Be(5);
        value.Name.Should().Be("Boost Time");
    }

    [Fact]
    public void BuildExportData_WithEmptyParamSet_ReturnsParamSetWithEmptyValues()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER")
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        var paramSet = result.ParamSetValues.Single();
        paramSet.ParamSetKey.Should().Be("MASTER");
        paramSet.Values.Should().BeEmpty();
    }

    [Fact]
    public void BuildExportData_WithDescriptionIdEqualToName_SetsNameToNull()
    {
        // Arrange – same name used for key and description id → dedup to null
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("VALUES", p => p.Add("ACTIVE", true, "ACTIVE"))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Single().Values.Single().Name.Should().BeNull();
    }

    [Fact]
    public void BuildExportData_WithDescriptionIdDifferentFromName_SetsNameToDescriptionId()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("VALUES", p => p.Add("BOOST_TIME", 5, "Boost Time"))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Single().Values.Single().Name.Should().Be("Boost Time");
    }

    [Fact]
    public void BuildExportData_WithDescriptionIdNull_SetsNameToNull()
    {
        // Arrange – descriptionId null → Description.Id is null, Name is null
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("VALUES", p => p.Add("ACTIVE", true, descriptionId: null))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Single().Values.Single().Name.Should().BeNull();
    }

    [Fact]
    public void BuildExportData_WithChannelParamSetValues_MapsChannelParamSetValues()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithChannel(c => c.WithParamSet("VALUES",
                p => p.Add("SET_TEMPERATURE", 21.5, "Set Temperature")))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        var channel = result.Channels.Single();
        var paramSet = channel.ParamSetValues.Single();
        paramSet.ParamSetKey.Should().Be("VALUES");
        paramSet.Values.Single().Key.Should().Be("SET_TEMPERATURE");
    }

    // ---- Filter: BuildExportData + Options --------------------------------

    [Fact]
    public void BuildExportData_WithNullOptions_IncludesAllParamSets()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p.Add("BOOST_TIME", 5, "Boost Time"))
            .WithParamSet("VALUES", p => p.Add("SET_TEMPERATURE", 21.0, "Set Temperature"))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, null);

        // Assert
        result.ParamSetValues.Select(p => p.ParamSetKey)
            .Should().BeEquivalentTo("MASTER", "VALUES");
    }

    [Fact]
    public void BuildExportData_WithParamSetWhitelist_FiltersDeviceParamSets()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p.Add("BOOST_TIME", 5, "Boost Time"))
            .WithParamSet("LINK", p => p.Add("PEER_NEEDS_BURST", true, "Peer Needs Burst"))
            .Build();
        var options = new DeviceExportOptions { ParamSetWhitelist = ["MASTER"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.ParamSetValues.Select(p => p.ParamSetKey).Should().BeEquivalentTo("MASTER");
    }

    [Fact]
    public void BuildExportData_WithParamSetWhitelist_FiltersChannelParamSets()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithChannel(c => c
                .WithParamSet("VALUES", p => p.Add("SET_TEMPERATURE", 21.5, "Set Temperature"))
                .WithParamSet("LINK", p => p.Add("PEER_NEEDS_BURST", true, "Peer Needs Burst")))
            .Build();
        var options = new DeviceExportOptions { ParamSetWhitelist = ["VALUES"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.Channels.Single().ParamSetValues.Select(p => p.ParamSetKey)
            .Should().BeEquivalentTo("VALUES");
    }

    [Fact]
    public void BuildExportData_WithParamValueNameWhitelist_FiltersDeviceValues()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p
                .Add("BOOST_TIME", 5, "Boost Time")
                .Add("DECALCIFICATION_TIME", 22, "Decalcification Time")
                .Add("PARTY_MODE", false, "Party Mode"))
            .Build();
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["BOOST_TIME", "PARTY_MODE"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.ParamSetValues.Single().Values.Select(v => v.Key)
            .Should().BeEquivalentTo("BOOST_TIME", "PARTY_MODE");
    }

    [Fact]
    public void BuildExportData_WithParamValueNameWhitelist_FiltersChannelValues()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithChannel(c => c.WithParamSet("VALUES", p => p
                .Add("SET_TEMPERATURE", 21.5, "Set Temperature")
                .Add("ACTUAL_TEMPERATURE", 20.1, "Actual Temperature")
                .Add("VALVE_STATE", 45, "Valve State")))
            .Build();
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["SET_TEMPERATURE", "VALVE_STATE"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.Channels.Single().ParamSetValues.Single().Values.Select(v => v.Key)
            .Should().BeEquivalentTo("SET_TEMPERATURE", "VALVE_STATE");
    }

    [Fact]
    public void BuildExportData_WithBothWhitelists_AppliesBothFilters()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p
                .Add("BOOST_TIME", 5, "Boost Time")
                .Add("DECALCIFICATION_TIME", 22, "Decalcification Time"))
            .WithParamSet("LINK", p => p.Add("PEER_NEEDS_BURST", true, "Peer Needs Burst"))
            .Build();
        var options = new DeviceExportOptions
        {
            ParamSetWhitelist = ["MASTER"],
            ParamValueNameWhitelist = ["BOOST_TIME"]
        };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        var paramSet = result.ParamSetValues.Single();
        paramSet.ParamSetKey.Should().Be("MASTER");
        paramSet.Values.Select(v => v.Key).Should().BeEquivalentTo("BOOST_TIME");
    }

    [Theory]
    [InlineData("master")]
    [InlineData("Master")]
    [InlineData("MASTER")]
    public void BuildExportData_WithParamSetWhitelistCaseVariants_MatchesCaseInsensitively(string whitelistKey)
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p.Add("BOOST_TIME", 5, "Boost Time"))
            .Build();
        var options = new DeviceExportOptions { ParamSetWhitelist = [whitelistKey] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.ParamSetValues.Select(p => p.ParamSetKey).Should().BeEquivalentTo("MASTER");
    }

    [Theory]
    [InlineData("boost_time")]
    [InlineData("Boost_Time")]
    [InlineData("BOOST_TIME")]
    public void BuildExportData_WithParamValueNameWhitelistCaseVariants_MatchesCaseInsensitively(string whitelistName)
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p
                .Add("BOOST_TIME", 5, "Boost Time")
                .Add("DECALCIFICATION_TIME", 22, "Decalcification Time"))
            .Build();
        var options = new DeviceExportOptions { ParamValueNameWhitelist = [whitelistName] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.ParamSetValues.Single().Values.Select(v => v.Key)
            .Should().BeEquivalentTo("BOOST_TIME");
    }

    [Fact]
    public void BuildExportData_WithParamValueNameWhitelistFilteringAllValues_ReturnsParamSetWithEmptyValues()
    {
        // Arrange – paramset itself stays, but all its values are filtered out
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p
                .Add("BOOST_TIME", 5, "Boost Time")
                .Add("DECALCIFICATION_TIME", 22, "Decalcification Time"))
            .Build();
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["NONEXISTENT"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        var paramSet = result.ParamSetValues.Single();
        paramSet.ParamSetKey.Should().Be("MASTER");
        paramSet.Values.Should().BeEmpty();
    }

    [Theory]
    [InlineData(42)]
    [InlineData(3.14)]
    [InlineData(true)]
    [InlineData("on")]
    public void BuildExportData_WithVariousValueTypes_PreservesValue(object value)
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("VALUES", p => p.Add("VAL", value, "Val"))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device);

        // Assert
        result.ParamSetValues.Single().Values.Single().Value.Should().Be(value);
    }

    [Fact]
    public void BuildExportData_WithParamValueNameWhitelistNonMatching_ReturnsEmptyValues()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("VALUES", p => p.Add("SET_TEMPERATURE", 21.5, "Set Temperature"))
            .Build();
        var options = new DeviceExportOptions { ParamValueNameWhitelist = ["NONEXISTENT"] };
        var sut = new DeviceExporter();

        // Act
        var result = sut.BuildExportData(device, options);

        // Assert
        result.ParamSetValues.Single().Values.Should().BeEmpty();
    }

    // ---- ExportDeviceAsync / JSON -----------------------------------------

    [Fact]
    public async Task ExportDeviceAsync_WithValidDevice_ReturnsValidJson()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder().Build();
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);

        // Assert
        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExportDeviceAsync_WithDefaultOptions_UsesCamelCasePropertyNames()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p.Add("BOOST_TIME", 5, "Boost Time"))
            .WithChannel()
            .Build();
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);

        // Assert
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.TryGetProperty("name", out _).Should().BeTrue();
        root.TryGetProperty("address", out _).Should().BeTrue();
        root.TryGetProperty("deviceType", out _).Should().BeTrue();
        root.TryGetProperty("firmwareVersion", out _).Should().BeTrue();
        root.TryGetProperty("ccu", out _).Should().BeTrue();
        root.TryGetProperty("paramSetKeys", out _).Should().BeTrue();
        root.TryGetProperty("paramSetValues", out _).Should().BeTrue();
        root.TryGetProperty("channels", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExportDeviceAsync_WithWriteIndentedTrue_ReturnsIndentedJson()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder().Build();
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
        var device = new CompleteCcuDeviceFakeBuilder().Build();
        var options = new DeviceExportOptions { WriteIndented = false };
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device, options);

        // Assert
        json.Should().NotContain("\n");
    }

    [Fact]
    public async Task ExportDeviceAsync_WithNullOptions_ReturnsIndentedJson()
    {
        // Arrange – default behavior when no options are provided is indented
        var device = new CompleteCcuDeviceFakeBuilder().Build();
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);

        // Assert
        json.Should().Contain("\n");
    }

    [Fact]
    public async Task ExportDeviceAsync_WithNullParamValueName_OmitsNamePropertyFromJson()
    {
        // Arrange – descriptionId == name triggers Name=null, which should be omitted
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("VALUES", p => p.Add("ACTIVE", true, "ACTIVE"))
            .Build();
        var options = new DeviceExportOptions { WriteIndented = false };
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device, options);

        // Assert
        json.Should().NotContain("\"name\":null");
    }

    [Fact]
    public async Task ExportDeviceAsync_WithPopulatedDevice_RoundTripsToEquivalentStructure()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithName("RoundTripDevice")
            .WithAddress("RT001")
            .WithDeviceType("HM-CC-RT-DN")
            .WithFirmware("1.4")
            .WithParamSetKeys("MASTER", "VALUES")
            .WithParamSet("MASTER", p => p.Add("BOOST_TIME", 5, "Boost Time"))
            .WithChannel(c => c
                .WithAddress("RT001:1")
                .WithIndex(1)
                .WithParamSet("VALUES", p => p.Add("SET_TEMPERATURE", 21.5, "Set Temperature")))
            .Build();
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDeviceAsync(device);
        var deserialized = JsonSerializer.Deserialize<DeviceExportData>(
            json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("RoundTripDevice");
        deserialized.Address.Should().Be("RT001");
        deserialized.DeviceType.Should().Be("HM-CC-RT-DN");
        deserialized.FirmwareVersion.Should().Be("1.4");
        deserialized.ParamSetKeys.Should().BeEquivalentTo("MASTER", "VALUES");

        var paramSet = deserialized.ParamSetValues.Single();
        paramSet.ParamSetKey.Should().Be("MASTER");
        paramSet.Values.Single().Key.Should().Be("BOOST_TIME");

        var channel = deserialized.Channels.Single();
        channel.Address.Should().Be("RT001:1");
        channel.Index.Should().Be(1);
        channel.ParamSetValues.Single().Values.Single().Key.Should().Be("SET_TEMPERATURE");
    }

    // ---- ExportDevicesAsync / JSON ----------------------------------------

    [Fact]
    public async Task ExportDevicesAsync_WithEmptyList_ReturnsEmptyJsonArray()
    {
        // Arrange
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([]);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        document.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task ExportDevicesAsync_WithSingleDevice_ReturnsArrayWithOneElement()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder().WithName("OnlyDevice").Build();
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([device]);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        document.RootElement.GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task ExportDevicesAsync_WithMultipleDevices_PreservesOrder()
    {
        // Arrange
        var device1 = new CompleteCcuDeviceFakeBuilder().WithName("Device1").WithAddress("ADDR1").Build();
        var device2 = new CompleteCcuDeviceFakeBuilder().WithName("Device2").WithAddress("ADDR2").Build();
        var device3 = new CompleteCcuDeviceFakeBuilder().WithName("Device3").WithAddress("ADDR3").Build();
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([device1, device2, device3]);

        // Assert
        var deserialized = JsonSerializer.Deserialize<List<DeviceExportData>>(
            json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deserialized.Should().NotBeNull();
        deserialized!.Select(d => d.Name).Should().ContainInOrder("Device1", "Device2", "Device3");
    }

    [Fact]
    public async Task ExportDevicesAsync_WithOptions_AppliesFilterToEachDevice()
    {
        // Arrange
        var device = new CompleteCcuDeviceFakeBuilder()
            .WithParamSet("MASTER", p => p
                .Add("BOOST_TIME", 5, "Boost Time")
                .Add("DECALCIFICATION_TIME", 22, "Decalcification Time"))
            .WithParamSet("LINK", p => p.Add("PEER_NEEDS_BURST", true, "Peer Needs Burst"))
            .Build();
        var options = new DeviceExportOptions
        {
            ParamSetWhitelist = ["MASTER"],
            ParamValueNameWhitelist = ["BOOST_TIME"]
        };
        var sut = new DeviceExporter();

        // Act
        var json = await sut.ExportDevicesAsync([device], options);

        // Assert
        var deserialized = JsonSerializer.Deserialize<List<DeviceExportData>>(
            json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deserialized.Should().NotBeNull();
        var paramSet = deserialized!.Single().ParamSetValues.Single();
        paramSet.ParamSetKey.Should().Be("MASTER");
        paramSet.Values.Select(v => v.Key).Should().BeEquivalentTo("BOOST_TIME");
    }
}
