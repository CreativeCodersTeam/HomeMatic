using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
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

    public async Task<IEnumerable<ParamSetValue>> GetParamSetValuesAsync(string paramSetKey)
    {
        var paramSets = await api.GetParamSetAsync(Uri.Address, paramSetKey).ConfigureAwait(false);

        return paramSets.Select(x => new ParamSetValue
        {
            Name = x.Key,
            Value = x.Value
        });
    }

    public async Task<CcuParameterDescriptions> GetParamSetDescriptionsAsync(string paramSetKey)
    {
        var paramSetDescriptions =
            await api.GetParameterDescriptionAsync(Uri.Address, paramSetKey).ConfigureAwait(false);

        return new CcuParameterDescriptions()
        {
            ParamSetKey = paramSetKey,
            Items = paramSetDescriptions
                .Select(x => x.Value)
                .Select(x => new CcuParameterDescription
                {
                    Id = x.Id,
                    DefaultValue = x.DefaultValue,
                    MinValue = x.MinValue,
                    MaxValue = x.MaxValue,
                    Type = x.Type,
                    DataType = x.DataType,
                    Unit = x.Unit,
                    TabOrder = x.TabOrder,
                    Control = x.Control,
                    ValuesList = x.ValuesList,
                    SpecialValues = x.SpecialValues
                })
        };
    }
}
