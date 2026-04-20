using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Parameters;

/// <summary>
/// Defines the well-known parameter-set key names used by the CCU.
/// </summary>
[PublicAPI]
public static class ParamSetKey
{
    /// <summary>
    /// The parameter set containing master (configuration) parameters.
    /// </summary>
    public const string Master = "MASTER";

    /// <summary>
    /// The parameter set containing value (state) parameters.
    /// </summary>
    public const string Values = "VALUES";

    /// <summary>
    /// The parameter set containing link (direct device link) parameters.
    /// </summary>
    public const string Link = "LINK";

    /// <summary>
    /// The parameter set containing service parameters.
    /// </summary>
    public const string Service = "SERVICE";

    /// <summary>
    /// All known parameter-set keys.
    /// </summary>
    public static readonly string[] ParamSetKeys = {Master, Values, Link, Service};

    /// <summary>
    /// Converts a parameter-set key string into the corresponding <see cref="ParameterKind"/> value.
    /// </summary>
    /// <param name="text">The parameter-set key name. The comparison is case-insensitive.</param>
    /// <returns>The matching <see cref="ParameterKind"/>, or <see cref="ParameterKind.Undefined"/> if no match is found.</returns>
    public static ParameterKind StringToParameterKind(string text)
    {
        return text.ToUpper() switch
        {
            Master => ParameterKind.Master,
            Values => ParameterKind.Values,
            Link => ParameterKind.Link,
            Service => ParameterKind.Service,
            _ => ParameterKind.Undefined
        };
    }
}
