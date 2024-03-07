namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDeviceDescription(ICcuDevice ccuDevice) : ICcuDevice
{
    public CcuSystemInfo CcuSystem => ccuDevice.CcuSystem;

    public string Name => ccuDevice.Name;

    public string Address => ccuDevice.Address;

    public string DeviceType => ccuDevice.DeviceType;

    public string[] ParamSets => ccuDevice.ParamSets;
}
