using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Describes the metadata of a single HomeMatic parameter as reported by the CCU.
/// </summary>
public class CcuParameterDescription
{
    /// <summary>
    /// Gets the identifier of the parameter.
    /// </summary>
    /// <value>The parameter identifier, or <see langword="null"/> if not provided.</value>
    public required string? Id { get; init; }

    /// <summary>
    /// Gets the default value of the parameter.
    /// </summary>
    /// <value>The default value, or <see langword="null"/> if not provided.</value>
    public required object? DefaultValue { get; init; }

    /// <summary>
    /// Gets the minimum value allowed for the parameter.
    /// </summary>
    /// <value>The minimum value, or <see langword="null"/> if not provided.</value>
    public required object? MinValue { get; init; }

    /// <summary>
    /// Gets the maximum value allowed for the parameter.
    /// </summary>
    /// <value>The maximum value, or <see langword="null"/> if not provided.</value>
    public required object? MaxValue { get; init; }

    /// <summary>
    /// Gets the raw type string as reported by the CCU.
    /// </summary>
    /// <value>The type string, or <see langword="null"/> if not provided.</value>
    public required string? Type { get; init; }

    /// <summary>
    /// Gets the strongly-typed data type of the parameter.
    /// </summary>
    /// <value>One of the enumeration values that specifies the data type.</value>
    public required ParameterDataType DataType { get; init; }

    /// <summary>
    /// Gets the unit string of the parameter value.
    /// </summary>
    /// <value>The unit string, or <see langword="null"/> if not provided.</value>
    public required string? Unit { get; init; }

    /// <summary>
    /// Gets the tab order used when displaying the parameter in the CCU UI.
    /// </summary>
    /// <value>The tab-order index.</value>
    public required int TabOrder { get; init; }

    /// <summary>
    /// Gets the control hint used for the parameter in the CCU UI.
    /// </summary>
    /// <value>The control hint string, or <see langword="null"/> if not provided.</value>
    public required string? Control { get; init; }

    /// <summary>
    /// Gets the list of allowed enum value names when the parameter is of enum type.
    /// </summary>
    /// <value>The enumerable of enum value names.</value>
    public required IEnumerable<string> ValuesList { get; init; } = [];

    /// <summary>
    /// Gets the special values associated with the parameter (for example, error or invalid markers).
    /// </summary>
    /// <value>The enumerable of special-value dictionaries keyed by name.</value>
    public required IEnumerable<Dictionary<string, object>> SpecialValues { get; init; } = [];
}
