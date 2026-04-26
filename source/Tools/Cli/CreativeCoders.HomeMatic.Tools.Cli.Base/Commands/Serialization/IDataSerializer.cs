namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;

/// <summary>
/// Serializes arbitrary data into a string representation for a specific output format.
/// </summary>
public interface IDataSerializer
{
    /// <summary>
    /// Gets the format produced by this serializer.
    /// </summary>
    DataOutputFormat Format { get; }

    /// <summary>
    /// Serializes the given <paramref name="data"/> to its string representation.
    /// </summary>
    /// <param name="data">The data to serialize.</param>
    /// <returns>The serialized representation of the data.</returns>
    string Serialize(object data);
}
