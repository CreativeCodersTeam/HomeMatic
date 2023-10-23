using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.JsonRpc.Models;

[UsedImplicitly]
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
}