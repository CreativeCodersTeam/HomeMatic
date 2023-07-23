using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;

namespace CreativeCoders.HomeMatic.XmlRpc.Converters
{
    public class DeviceFirmwareUpdateStateValueConverter : IXmlRpcMemberValueConverter
    {
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

        public XmlRpcValue ConvertFromObject(object value)
        {
            throw new System.NotImplementedException();
        }
    }
}