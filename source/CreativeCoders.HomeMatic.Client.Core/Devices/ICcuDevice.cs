namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public interface ICcuDevice
{
    CcuSystemInfo? CcuSystem { get; }
    
    string? Name { get; }

    string? Address { get; }

    string? DeviceType { get; }
}
