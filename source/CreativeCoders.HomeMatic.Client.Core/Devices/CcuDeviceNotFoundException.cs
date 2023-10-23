namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDeviceNotFoundException : Exception
{
    public CcuDeviceNotFoundException(string deviceAddress) : base($"Device with address '{deviceAddress}' not found")
    {
        DeviceAddress = deviceAddress;
    }

    public string DeviceAddress { get; set; }
}
