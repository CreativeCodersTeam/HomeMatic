using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Abstractions;

public interface ICcuDeviceBase
{
    CcuDeviceUri Uri { get; }

    string DeviceType { get; }

    bool IsAesActive { get; }

    string Interface { get; }

    int Version { get; }

    bool Roaming { get; }

    string[] ParamSets { get; }

    Task<IEnumerable<IParamSetValue>> GetParamSetValuesAsync(string paramSetKey);
}
