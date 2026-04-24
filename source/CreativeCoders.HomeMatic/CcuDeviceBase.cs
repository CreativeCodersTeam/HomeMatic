using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Provides the shared base implementation of <see cref="ICcuDeviceBase"/> for HomeMatic devices and channels.
/// </summary>
/// <param name="api">The XML-RPC API used to query parameter-set values and descriptions from the CCU.</param>
public abstract class CcuDeviceBase(IHomeMaticXmlRpcApi api) : ICcuDeviceBase
{
    /// <inheritdoc />
    public required CcuDeviceUri Uri { get; init; }

    /// <inheritdoc />
    public required string DeviceType { get; init; }

    /// <inheritdoc />
    public required bool IsAesActive { get; init; }

    /// <inheritdoc />
    public required string Interface { get; init; }

    /// <inheritdoc />
    public required int Version { get; init; }

    /// <inheritdoc />
    public required bool Roaming { get; init; }

    /// <inheritdoc />
    public required string[] ParamSets { get; init; }

    /// <inheritdoc />
    public async Task<IEnumerable<ParamSetValue>> GetParamSetValuesAsync(string paramSetKey)
    {
        var paramSets = await api.GetParamSetAsync(Uri.Address, paramSetKey).ConfigureAwait(false);

        return paramSets.Select(x => new ParamSetValue
        {
            Name = x.Key,
            Value = x.Value
        });
    }

    /// <inheritdoc />
    public async Task<CcuParameterDescriptions> GetParamSetDescriptionsAsync(string paramSetKey)
    {
        var paramSetDescriptions =
            await api.GetParameterDescriptionAsync(Uri.Address, paramSetKey).ConfigureAwait(false);

        return new CcuParameterDescriptions
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
