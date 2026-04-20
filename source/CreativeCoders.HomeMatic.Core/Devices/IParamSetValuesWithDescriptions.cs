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
}
