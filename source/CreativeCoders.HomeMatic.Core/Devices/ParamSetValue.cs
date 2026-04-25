namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Represents a single named parameter value within a parameter set.
/// </summary>
public class ParamSetValue
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    /// <value>The parameter name.</value>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the current value of the parameter.
    /// </summary>
    /// <value>The parameter value.</value>
    public required object Value { get; init; }
}
