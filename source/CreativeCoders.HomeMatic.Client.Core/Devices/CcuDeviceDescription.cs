namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDeviceDescription : ICcuDevice
{
    private readonly ICcuDevice _ccuDevice;

    public CcuDeviceDescription(ICcuDevice ccuDevice)
    {
        _ccuDevice = ccuDevice;
    }

    public CcuSystemInfo CcuSystem => _ccuDevice.CcuSystem;

    public string Name => _ccuDevice.Name;

    public string Address => _ccuDevice.Address;

    public string DeviceType => _ccuDevice.DeviceType;

    public string[] ParamSets => _ccuDevice.ParamSets;
}
