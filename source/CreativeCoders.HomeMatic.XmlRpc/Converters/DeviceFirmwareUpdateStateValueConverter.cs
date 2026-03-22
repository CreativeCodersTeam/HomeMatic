using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters;

/// <summary>
/// Converts the string-encoded firmware update state returned by the HomeMatic CCU into a <see cref="DeviceFirmwareUpdateState"/> enum value.
/// </summary>
/// <remarks>
/// The CCU reports the firmware update state of a device as a string in the <c>FIRMWARE_UPDATE_STATE</c>
/// field of the device description. This converter maps the known string values to the corresponding
/// <see cref="DeviceFirmwareUpdateState"/> enum members. Any unrecognized string is mapped to
/// <see cref="DeviceFirmwareUpdateState.None"/>.
/// </remarks>
public class DeviceFirmwareUpdateStateValueConverter : IXmlRpcMemberValueConverter
{
    /// <summary>
    /// Converts an <see cref="XmlRpcValue"/> containing a firmware update state string into a
    /// <see cref="DeviceFirmwareUpdateState"/> value.
    /// </summary>
    /// <param name="xmlRpcValue">The XML-RPC value to convert.</param>
    /// <returns>The corresponding <see cref="DeviceFirmwareUpdateState"/>, or <see cref="DeviceFirmwareUpdateState.None"/> if the value is unrecognized.</returns>
    public object ConvertFromValue(XmlRpcValue xmlRpcValue)
    {
        if (xmlRpcValue is not StringValue text)
        {
            return DeviceFirmwareUpdateState.None;
        }

        return text.Value.ToUpper() switch
        {
            "UP_TO_DATE" => DeviceFirmwareUpdateState.UpToDate,
            "NEW_FIRMWARE_AVAILABLE" => DeviceFirmwareUpdateState.NewFirmwareAvailable,
            "DELIVER_FIRMWARE_IMAGE" => DeviceFirmwareUpdateState.DeliverFirmwareImage,
            "READY_FOR_UPDATE" => DeviceFirmwareUpdateState.ReadyForUpdate,
            "PERFORMING_UPDATE" => DeviceFirmwareUpdateState.PerformingUpdate,
            _ => DeviceFirmwareUpdateState.None
        };
    }

    /// <summary>
    /// Converts a <see cref="DeviceFirmwareUpdateState"/> value into an <see cref="XmlRpcValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>This method is not implemented and always throws <see cref="System.NotImplementedException"/>.</returns>
    /// <exception cref="System.NotImplementedException">Always thrown; serialization of this type is not supported.</exception>
    public XmlRpcValue ConvertFromObject(object value)
    {
        throw new System.NotImplementedException();
    }
}