using System.Text.Json;
using System.Text.Json.Serialization;

namespace CreativeCoders.HomeMatic.JsonRpc;

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

public class BooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.String:
                return reader.GetString() switch
                {
                    "true" => true,
                    "false" => false,
                    _ => throw new JsonException()
                };
            default:
                throw new JsonException();
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
