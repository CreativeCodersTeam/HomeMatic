using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Parameters;

/// <summary>
/// Specifies the data type of a HomeMatic parameter value.
/// </summary>
[PublicAPI]
public enum ParameterDataType
{
    /// <summary>
    /// The data type is unknown or not yet determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// A signed integer value.
    /// </summary>
    Integer,

    /// <summary>
    /// A boolean value.
    /// </summary>
    Bool,

    /// <summary>
    /// A floating-point value.
    /// </summary>
    Float,

    /// <summary>
    /// An action trigger value (write-only, with no meaningful stored state).
    /// </summary>
    Action,

    /// <summary>
    /// An enumeration value represented as an integer index.
    /// </summary>
    Enum,

    /// <summary>
    /// A textual value.
    /// </summary>
    String
}
