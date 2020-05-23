using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Devices
{
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
}