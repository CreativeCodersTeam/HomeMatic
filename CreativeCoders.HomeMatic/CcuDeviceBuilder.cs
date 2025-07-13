using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class CcuDeviceBuilder
{
    private CcuDeviceUri? _uri;

    private DeviceDescription? _deviceDescription;

    private IHomeMaticXmlRpcApi? _api;

    public CcuDeviceBuilder WithUri(CcuDeviceUri deviceUri)
    {
        _uri = deviceUri;

        return this;
    }

    public CcuDeviceBuilder WithApi(IHomeMaticXmlRpcApi api)
    {
        _api = api;

        return this;
    }

    public CcuDeviceBuilder FromDeviceDescription(DeviceDescription deviceDescription)
    {
        _deviceDescription = deviceDescription;

        return this;
    }

    public CcuDevice Build()
    {
        if (_uri == null || _api == null)
        {
            throw new InvalidOperationException("Uri and Api must be set");
        }

        var ccuDevice = new CcuDevice(_api)
        {
            Uri = _uri,
            DeviceType = _deviceDescription?.DeviceType ?? string.Empty,
            Version = _deviceDescription?.Version ?? 0,
            IsAesActive = _deviceDescription?.IsAesActive ?? false,
            Interface = _deviceDescription?.Interface ?? string.Empty,
            RxMode = _deviceDescription?.RxMode ?? RxMode.None,
            RfAddress = _deviceDescription?.RfAddress ?? 0,
            Firmware = _deviceDescription?.Firmware ?? string.Empty,
            AvailableFirmware = _deviceDescription?.AvailableFirmware ?? string.Empty,
            CanBeUpdated = _deviceDescription?.CanBeUpdated ?? false,
            FirmwareUpdateState = _deviceDescription?.FirmwareUpdateState ?? DeviceFirmwareUpdateState.None,
            Roaming = _deviceDescription?.Roaming ?? false,
            ParamSets = _deviceDescription?.ParamSets ?? []
        };

        return ccuDevice;
    }
}
