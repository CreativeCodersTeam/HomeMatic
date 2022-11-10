using CreativeCoders.HomeMatic.Api.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Devices;

[PublicAPI]
public abstract class CcuDeviceBase : ICcuDeviceBase
{
    protected CcuDeviceBase(ICcuDeviceInfo deviceInfo, IHomeMaticXmlRpcApi xmlRpcApi)
    {
        DeviceInfo = deviceInfo;
        XmlRpcApi = xmlRpcApi;
    }

    protected ICcuDeviceInfo DeviceInfo { get; }

    protected IHomeMaticXmlRpcApi XmlRpcApi { get; }

    public string Address => DeviceInfo.Address;

    public string DeviceType => DeviceInfo.DeviceType;

    public int Version => DeviceInfo.Version;

    public bool IsAesActive => DeviceInfo.IsAesActive;

    public string[] ParamSets => DeviceInfo.ParamSets;

    public bool Roaming => DeviceInfo.Roaming;
        
    public abstract bool IsDevice { get; }

    public bool IsChannel => !IsDevice;
}