using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;

/// <summary>
/// Default <see cref="IDataSerializerFactory"/> implementation backed by all registered
/// <see cref="IDataSerializer"/> instances.
/// </summary>
public class DataSerializerFactory : IDataSerializerFactory
{
    private readonly Dictionary<DataOutputFormat, IDataSerializer> _serializers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSerializerFactory"/> class.
    /// </summary>
    /// <param name="serializers">All registered serializers.</param>
    public DataSerializerFactory(IEnumerable<IDataSerializer> serializers)
    {
        _serializers = Ensure.NotNull(serializers).ToDictionary(s => s.Format);
    }

    /// <inheritdoc />
    public IDataSerializer Create(DataOutputFormat format)
    {
        if (format == DataOutputFormat.Auto)
        {
            throw new ArgumentException(
                "Format must be resolved to a concrete value before requesting a serializer.",
                nameof(format));
        }

        if (!_serializers.TryGetValue(format, out var serializer))
        {
            throw new InvalidOperationException(
                $"No serializer registered for format '{format}'.");
        }

        return serializer;
    }
}
