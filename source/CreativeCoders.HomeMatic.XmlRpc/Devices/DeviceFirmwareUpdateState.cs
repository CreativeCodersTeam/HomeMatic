using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Devices;

/// <summary>
/// Specifies the firmware update state of a device.
/// </summary>
[PublicAPI]
public enum DeviceFirmwareUpdateState
{
    /// <summary>
    /// No firmware update state is reported.
    /// </summary>
    None,

    /// <summary>
    /// The device firmware is up to date.
    /// </summary>
    UpToDate,

    /// <summary>
    /// A new firmware version is available for the device.
    /// </summary>
    NewFirmwareAvailable,

    /// <summary>
    /// The firmware image is being delivered to the device.
    /// </summary>
    DeliverFirmwareImage,

    /// <summary>
    /// The device is ready to perform the firmware update.
    /// </summary>
    ReadyForUpdate,

    /// <summary>
    /// The firmware update is currently being performed.
    /// </summary>
    PerformingUpdate
}
