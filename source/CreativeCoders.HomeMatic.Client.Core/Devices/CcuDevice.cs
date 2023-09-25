namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDevice : CcuDeviceBase
{
    public CcuSystem? CcuSystem { get; set; }
    
    public string? Name { get; init; }

    public string? Address { get; set; }

    public string? DeviceType { get; set; }
}
