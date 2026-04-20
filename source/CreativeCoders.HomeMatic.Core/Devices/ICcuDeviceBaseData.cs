namespace CreativeCoders.HomeMatic.Core.Devices;

/// <summary>
/// Defines the common identifying and structural data of a HomeMatic device or channel.
/// </summary>
public interface ICcuDeviceBaseData
{
    /// <summary>
    /// Gets the URI that uniquely identifies the device or channel on its CCU.
    /// </summary>
    /// <value>The <see cref="CcuDeviceUri"/> of the device or channel.</value>
    CcuDeviceUri Uri { get; }

    /// <summary>
    /// Gets the device type identifier as reported by the CCU.
    /// </summary>
    /// <value>The device type string.</value>
    string DeviceType { get; }

    /// <summary>
    /// Gets a value that indicates whether AES signing is active for this device or channel.
    /// </summary>
    /// <value><see langword="true"/> if AES signing is active; otherwise, <see langword="false"/>.</value>
    bool IsAesActive { get; }

    /// <summary>
    /// Gets the interface identifier the device is attached to.
    /// </summary>
    /// <value>The interface identifier.</value>
    string Interface { get; }

    /// <summary>
    /// Gets the version of the device description.
    /// </summary>
    /// <value>The version number.</value>
    int Version { get; }

    /// <summary>
    /// Gets a value that indicates whether the device supports interface roaming.
    /// </summary>
    /// <value><see langword="true"/> if the device supports roaming; otherwise, <see langword="false"/>.</value>
    bool Roaming { get; }

    /// <summary>
    /// Gets the parameter-set keys available for this device or channel.
    /// </summary>
    /// <value>The array of parameter-set keys.</value>
    string[] ParamSets { get; }
}
