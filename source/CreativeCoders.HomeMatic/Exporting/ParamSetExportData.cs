namespace CreativeCoders.HomeMatic.Exporting;

/// <summary>
/// Represents the exported values of a single parameter set of a device or channel.
/// </summary>
public class ParamSetExportData
{
    /// <summary>
    /// Gets the key of the parameter set these values belong to.
    /// </summary>
    /// <value>The parameter-set key (for example <c>"MASTER"</c> or <c>"VALUES"</c>).</value>
    public required string ParamSetKey { get; init; }

    /// <summary>
    /// Gets the individual parameter values of this parameter set.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamValueExportData"/> entries.</value>
    public required IEnumerable<ParamValueExportData> Values { get; init; }
}
