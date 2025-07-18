namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICcuDeviceBase : ICcuDeviceBaseData
{
    Task<IEnumerable<ParamSetValue>> GetParamSetValuesAsync(string paramSetKey);

    Task<CcuParameterDescriptions> GetParamSetDescriptionsAsync(string paramSetKey);
}

public class CcuParameterDescriptions
{
    public required string ParamSetKey { get; init; }

    public required IEnumerable<CcuParameterDescription> Items { get; init; }
}
