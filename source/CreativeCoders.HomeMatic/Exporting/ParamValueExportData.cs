namespace CreativeCoders.HomeMatic.Exporting;

/// <summary>
/// Represents a single exported parameter value.
/// </summary>
public class ParamValueExportData
{
    /// <summary>
    /// Gets the technical key of the parameter (for example <c>"SET_TEMPERATURE"</c>).
    /// </summary>
    /// <value>The parameter key.</value>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the descriptive name of the parameter when it differs from <see cref="Key"/>.
    /// </summary>
    /// <value>The parameter name, or <see langword="null"/> if it is identical to <see cref="Key"/>.</value>
    public required string? Name { get; init; }

    /// <summary>
    /// Gets the current value of the parameter.
    /// </summary>
    /// <value>The parameter value as reported by the CCU.</value>
    public required object Value { get; init; }
}
