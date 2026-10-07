using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.JsonRpc.Models;

[UsedImplicitly]
[PublicAPI]
public class DeviceDetails
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Interface { get; set; }

    public string? Type { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool OperateGroupOnly { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool IsReady { get; set; }

    /// <summary>
    /// Gets or sets the channels of the device.
    /// </summary>
    /// <value>The channels returned by <c>Device.listAllDetail</c>, or <see langword="null"/> if the CCU returned none.</value>
    public ChannelDetails[]? Channels { get; set; }
}
