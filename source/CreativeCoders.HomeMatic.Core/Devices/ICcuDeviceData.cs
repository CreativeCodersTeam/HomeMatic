using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;

namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Defines the device-level data of a HomeMatic device.
/// </summary>
public interface ICcuDeviceData : ICcuDeviceBaseData
{
    /// <summary>
    /// Gets the human-readable name of the device.
    /// </summary>
    /// <value>The device name.</value>
    string Name { get; }

    /// <summary>
    /// Gets the reception mode flags for the device.
    /// </summary>
    /// <value>A bitwise combination of the enumeration values that specifies the reception mode.</value>
    RxModes RxMode { get; }

    /// <summary>
    /// Gets the RF address of the device on the BidCoS radio bus.
    /// </summary>
    /// <value>The numeric RF address.</value>
    int RfAddress { get; }

    /// <summary>
    /// Gets the currently installed firmware version.
    /// </summary>
    /// <value>The firmware version string.</value>
    string Firmware { get; }

    /// <summary>
    /// Gets the firmware version available for update.
    /// </summary>
    /// <value>The available firmware version string, or empty if no update is available.</value>
    string AvailableFirmware { get; }

    /// <summary>
    /// Gets a value that indicates whether the device firmware can be updated.
    /// </summary>
    /// <value><see langword="true"/> if the device supports firmware updates; otherwise, <see langword="false"/>.</value>
    bool CanBeUpdated { get; }

    /// <summary>
    /// Gets the current firmware update state of the device.
    /// </summary>
    /// <value>One of the enumeration values that specifies the firmware update state.</value>
    DeviceFirmwareUpdateState FirmwareUpdateState { get; }
}
