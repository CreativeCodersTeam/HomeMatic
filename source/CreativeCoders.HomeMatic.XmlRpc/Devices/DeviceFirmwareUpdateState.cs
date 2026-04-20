using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Devices;

[PublicAPI]
public enum DeviceFirmwareUpdateState
{
    None,
    UpToDate,
    NewFirmwareAvailable,
    DeliverFirmwareImage,
    ReadyForUpdate,
    PerformingUpdate
}