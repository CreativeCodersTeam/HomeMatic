namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;

/// <summary>
/// Creates <see cref="IDataSerializer"/> instances for a requested <see cref="DataOutputFormat"/>.
/// </summary>
public interface IDataSerializerFactory
{
    /// <summary>
    /// Returns the <see cref="IDataSerializer"/> registered for the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The desired output format. Must not be <see cref="DataOutputFormat.Auto"/>.</param>
    /// <returns>A serializer that produces output in <paramref name="format"/>.</returns>
    IDataSerializer Create(DataOutputFormat format);
}
