namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICcuDeviceBaseData
{
    CcuDeviceUri Uri { get; }

    string DeviceType { get; }

    bool IsAesActive { get; }

    string Interface { get; }

    int Version { get; }

    bool Roaming { get; }

    string[] ParamSets { get; }
}
