using System.Collections.Generic;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Groups the values and descriptions that belong to a single parameter set.
/// </summary>
public class ParamSetValuesWithDescriptions
{
    /// <summary>
    /// Gets the key of the parameter set.
    /// </summary>
    /// <value>The parameter-set key.</value>
    public required string ParamSetKey { get; init; }

    /// <summary>
    /// Gets the parameter values along with their descriptions.
    /// </summary>
    /// <value>The enumerable of <see cref="ParamSetValueWithDescription"/> entries.</value>
    public required IEnumerable<ParamSetValueWithDescription> ParamSetValues { get; init; }

    /// <summary>
    /// Gets the error message if reading the parameter-set values from the CCU failed.
    /// </summary>
    /// <value>The error message, or <see langword="null"/> if the values were read successfully.</value>
    public string? ReadError { get; init; }
}
