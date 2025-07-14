namespace CreativeCoders.HomeMatic.Abstractions.Devices;

public interface ICcuDeviceBase : ICcuDeviceBaseData
{
    Task<IEnumerable<IParamSetValue>> GetParamSetValuesAsync(string paramSetKey);
}
