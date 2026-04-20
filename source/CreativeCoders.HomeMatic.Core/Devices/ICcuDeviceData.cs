using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;

namespace CreativeCoders.HomeMatic.Core.Devices;

public interface ICcuDeviceData : ICcuDeviceBaseData
{
    string Name { get; }

    RxMode RxMode { get; }

    int RfAddress { get; }

    string Firmware { get; }

    string AvailableFirmware { get; }

    bool CanBeUpdated { get; }

    DeviceFirmwareUpdateState FirmwareUpdateState { get; }
}
