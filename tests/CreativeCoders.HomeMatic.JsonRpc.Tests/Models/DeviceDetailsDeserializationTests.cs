using System.Text.Json;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.JsonRpc.Models;

namespace CreativeCoders.HomeMatic.JsonRpc.Tests.Models;

public class DeviceDetailsDeserializationTests
{
    // The JSON-RPC client reads responses with HttpContent.ReadFromJsonAsync without explicit options,
    // which uses the web defaults (camelCase, case-insensitive property names).
    private static readonly JsonSerializerOptions JsonRpcClientOptions = JsonSerializerOptions.Web;

    [Fact]
    public void Deserialize_DeviceWithChannels_FillsChannelNameAndAddress()
    {
        // Arrange
        const string json =
            """
            {
              "id": "1234",
              "name": "Living room light",
              "address": "000A1B2C3D4E5F",
              "interface": "HmIP-RF",
              "type": "HmIP-BSM",
              "operateGroupOnly": "false",
              "isReady": "true",
              "channels": [
                { "id": "1235", "name": "Living room light:1", "address": "000A1B2C3D4E5F:1" }
              ]
            }
            """;

        // Act
        var deviceDetails = JsonSerializer.Deserialize<DeviceDetails>(json, JsonRpcClientOptions);

        // Assert
        deviceDetails.Should().NotBeNull();
        deviceDetails.Channels.Should().ContainSingle();
        deviceDetails.Channels[0].Id.Should().Be("1235");
        deviceDetails.Channels[0].Name.Should().Be("Living room light:1");
        deviceDetails.Channels[0].Address.Should().Be("000A1B2C3D4E5F:1");
    }

    [Fact]
    public void Deserialize_DeviceWithoutChannels_ChannelsIsNull()
    {
        // Arrange
        const string json =
            """
            { "id": "1234", "name": "Living room light", "address": "000A1B2C3D4E5F", "isReady": "true" }
            """;

        // Act
        var deviceDetails = JsonSerializer.Deserialize<DeviceDetails>(json, JsonRpcClientOptions);

        // Assert
        deviceDetails.Should().NotBeNull();
        deviceDetails.Channels.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ChannelWithUnknownFields_IgnoresUnknownFields()
    {
        // Arrange
        const string json =
            """
            {
              "address": "000A1B2C3D4E5F",
              "channels": [
                {
                  "id": "1235",
                  "name": "Living room light:1",
                  "address": "000A1B2C3D4E5F:1",
                  "isReady": "true",
                  "category": "CATEGORY_SENDER",
                  "channelType": "SWITCH_VIRTUAL_RECEIVER"
                }
              ]
            }
            """;

        // Act
        var deviceDetails = JsonSerializer.Deserialize<DeviceDetails>(json, JsonRpcClientOptions);

        // Assert
        deviceDetails.Should().NotBeNull();
        deviceDetails.Channels.Should().ContainSingle();
        deviceDetails.Channels[0].Name.Should().Be("Living room light:1");
        deviceDetails.Channels[0].Address.Should().Be("000A1B2C3D4E5F:1");
    }
}
