namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Pairs a parameter value with its corresponding description.
/// </summary>
public class ParamSetValueWithDescription
{
    /// <summary>
    /// Gets the parameter value.
    /// </summary>
    /// <value>The <see cref="Devices.ParamSetValue"/> instance.</value>
    public required ParamSetValue ParamSetValue { get; init; }

    /// <summary>
    /// Gets the description that belongs to the parameter.
    /// </summary>
    /// <value>The <see cref="CcuParameterDescription"/> instance.</value>
    public required CcuParameterDescription Description { get; init; }
}
