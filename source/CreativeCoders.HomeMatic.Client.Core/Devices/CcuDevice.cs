using CreativeCoders.HomeMatic.Api.Core.Devices;
using CreativeCoders.HomeMatic.Api.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client.Core.Devices;

public class CcuDevice(
    CcuSystemInfo ccuSystemInfo,
    string? name,
    DeviceDescription deviceDescription,
    IHomeMaticXmlRpcApi xmlRpcApi)
    : CcuDeviceBase, ICcuDevice
{
    public CcuSystemInfo? CcuSystem { get; set; } = ccuSystemInfo;

    public string Name { get; } = name ?? deviceDescription.Address;

    public string Address { get; } = deviceDescription.Address;

    public string DeviceType { get; } = deviceDescription.DeviceType;

    public string[] ParamSets { get; } = deviceDescription.ParamSets;

    public Task<Dictionary<string, object>> GetParamSetAsync(string paramSetName)
    {
        return xmlRpcApi.GetParamSetAsync(Address, paramSetName);
    }
}
