using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Defines the shared functionality for devices and channels, including access to their parameter sets.
/// </summary>
public interface ICcuDeviceBase : ICcuDeviceBaseData
{
    /// <summary>
    /// Asynchronously retrieves the current values for the specified parameter set.
    /// </summary>
    /// <param name="paramSetKey">The key of the parameter set to read.</param>
    /// <returns>A task that yields the enumerable of <see cref="ParamSetValue"/> entries in the parameter set.</returns>
    Task<IEnumerable<ParamSetValue>> GetParamSetValuesAsync(string paramSetKey);

    /// <summary>
    /// Asynchronously retrieves the descriptions for the specified parameter set.
    /// </summary>
    /// <param name="paramSetKey">The key of the parameter set whose descriptions should be read.</param>
    /// <returns>A task that yields a <see cref="CcuParameterDescriptions"/> grouping.</returns>
    Task<CcuParameterDescriptions> GetParamSetDescriptionsAsync(string paramSetKey);
}

/// <summary>
/// Groups the parameter descriptions that belong to a single parameter set.
/// </summary>
public class CcuParameterDescriptions
{
    /// <summary>
    /// Gets the key of the parameter set these descriptions belong to.
    /// </summary>
    /// <value>The parameter-set key.</value>
    public required string ParamSetKey { get; init; }

    /// <summary>
    /// Gets the parameter descriptions contained in this set.
    /// </summary>
    /// <value>The enumerable of <see cref="CcuParameterDescription"/> entries.</value>
    public required IEnumerable<CcuParameterDescription> Items { get; init; }
}
