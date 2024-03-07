namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDeviceNotFoundException(string deviceAddress)
    : Exception($"Device with address '{deviceAddress}' not found")
{
    public string DeviceAddress { get; set; } = deviceAddress;
}
