using System;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Parameters;

/// <summary>
/// Specifies the reception mode of a BidCoS-RF device.
/// </summary>
/// <remarks>
/// Values can be combined with bitwise OR.
/// </remarks>
[PublicAPI]
[Flags]
public enum RxMode
{
    /// <summary>
    /// No reception mode is set.
    /// </summary>
    None = 0,

    /// <summary>
    /// The device is always listening for incoming messages.
    /// </summary>
    Always = 1,

    /// <summary>
    /// The device listens only after a wake-up burst.
    /// </summary>
    Burst = 2,

    /// <summary>
    /// The device is in configuration reception mode.
    /// </summary>
    Config = 4,

    /// <summary>
    /// The device wakes up periodically to receive messages.
    /// </summary>
    WakeUp = 8,

    /// <summary>
    /// The device uses a lazy configuration mode to save battery.
    /// </summary>
    LazyConfig = 16
}
