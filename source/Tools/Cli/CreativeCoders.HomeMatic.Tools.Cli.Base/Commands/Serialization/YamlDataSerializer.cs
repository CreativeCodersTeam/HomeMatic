using CreativeCoders.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;

/// <summary>
/// <see cref="IDataSerializer"/> implementation that produces YAML using <c>YamlDotNet</c>.
/// </summary>
public class YamlDataSerializer : IDataSerializer
{
    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    /// <inheritdoc />
    public DataOutputFormat Format => DataOutputFormat.Yaml;

    /// <inheritdoc />
    public string Serialize(object data)
    {
        Ensure.NotNull(data);

        return Serializer.Serialize(data);
    }
}
