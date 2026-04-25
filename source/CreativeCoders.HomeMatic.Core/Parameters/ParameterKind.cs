namespace CreativeCoders.HomeMatic.Core.Parameters;

/// <summary>
/// Specifies the kind of parameter set that a parameter belongs to.
/// </summary>
public enum ParameterKind
{
    /// <summary>
    /// The parameter kind is not defined.
    /// </summary>
    Undefined,

    /// <summary>
    /// The parameter belongs to the master (configuration) parameter set.
    /// </summary>
    Master,

    /// <summary>
    /// The parameter belongs to the values (state) parameter set.
    /// </summary>
    Values,

    /// <summary>
    /// The parameter belongs to the link (direct device link) parameter set.
    /// </summary>
    Link,

    /// <summary>
    /// The parameter belongs to the service parameter set.
    /// </summary>
    Service
}
