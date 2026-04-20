using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Parameters;

/// <summary>
/// Specifies attribute flags for a HomeMatic parameter.
/// </summary>
/// <remarks>
/// Values can be combined with bitwise OR.
/// </remarks>
[PublicAPI]
[Flags]
public enum ParameterFlags
{
    /// <summary>
    /// No flags are set.
    /// </summary>
    None = 0,

    /// <summary>
    /// The parameter is visible to end users in the CCU UI.
    /// </summary>
    Visible = 1,

    /// <summary>
    /// The parameter is used internally and is not meant to be shown to end users.
    /// </summary>
    Internal = 2,

    /// <summary>
    /// The parameter requires a value transformation before use.
    /// </summary>
    Transform = 4,

    /// <summary>
    /// The parameter represents a service message or diagnostics value.
    /// </summary>
    Service = 8,

    /// <summary>
    /// The parameter is sticky — it must be acknowledged explicitly to be reset.
    /// </summary>
    Sticky = 16
}
