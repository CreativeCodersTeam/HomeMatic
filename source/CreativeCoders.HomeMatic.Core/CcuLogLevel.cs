using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core;

/// <summary>
/// Specifies the severity of log entries produced by the CCU.
/// </summary>
[PublicAPI]
public enum CcuLogLevel
{
    /// <summary>
    /// All log entries are included.
    /// </summary>
    All = 0,

    /// <summary>
    /// Detailed diagnostic messages used for debugging.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Informational messages that describe normal operation.
    /// </summary>
    Info = 2,

    /// <summary>
    /// Notable events that do not indicate a problem.
    /// </summary>
    Notice = 3,

    /// <summary>
    /// Conditions that may indicate a potential problem.
    /// </summary>
    Warning = 4,

    /// <summary>
    /// Errors that affect the current operation.
    /// </summary>
    Error = 5,

    /// <summary>
    /// Fatal errors that prevent further operation.
    /// </summary>
    FatalError = 6
}
