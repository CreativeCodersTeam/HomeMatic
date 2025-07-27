using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreativeCoders.HomeMatic.Core.Devices;

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
