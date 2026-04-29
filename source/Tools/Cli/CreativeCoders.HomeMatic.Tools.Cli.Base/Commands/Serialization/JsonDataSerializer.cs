using System.Text.Json;
using System.Text.Json.Serialization;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;

/// <summary>
/// <see cref="IDataSerializer"/> implementation that produces indented JSON using
/// <c>System.Text.Json</c>.
/// </summary>
public class JsonDataSerializer : IDataSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public DataOutputFormat Format => DataOutputFormat.Json;

    /// <inheritdoc />
    public string Serialize(object data)
    {
        Ensure.NotNull(data);

        return JsonSerializer.Serialize(data, Options);
    }
}
