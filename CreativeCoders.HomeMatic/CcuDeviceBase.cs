using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Abstractions.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public abstract class CcuDeviceBase(IHomeMaticXmlRpcApi api) : ICcuDeviceBase
{
    public required CcuDeviceUri Uri { get; init; }

    public required string DeviceType { get; init; }

    public required bool IsAesActive { get; init; }

    public required string Interface { get; init; }

    public required int Version { get; init; }

    public required bool Roaming { get; init; }

    public required string[] ParamSets { get; init; }

    public async Task<IEnumerable<IParamSetValue>> GetParamSetValuesAsync(string paramSetKey)
    {
        var paramSets = await api.GetParamSetAsync(Uri.Address, paramSetKey).ConfigureAwait(false);

        return paramSets.Select(x => new ParamSetValue
        {
            Name = x.Key,
            Value = x.Value
        });
    }
}
