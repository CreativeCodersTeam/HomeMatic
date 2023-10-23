using System.Collections.Generic;
using CreativeCoders.HomeMatic.Api.Core.Devices;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Api.Devices;

public class CcuDevice : CcuDeviceBase, ICcuDevice
{
    private readonly List<ICcuDeviceChannel> _channels;
        
    public CcuDevice(ICcuDeviceInfo deviceInfo, IHomeMaticXmlRpcApi xmlRpcApi) : base(deviceInfo, xmlRpcApi)
    {
        _channels = new List<ICcuDeviceChannel>();
    }

    public void AddChannels(IEnumerable<ICcuDeviceChannel> channels)
    {
        _channels.AddRange(channels);
    }

    public int RfAddress => DeviceInfo.RfAddress;

    public string Firmware => DeviceInfo.Firmware;

    public string AvailableFirmware => DeviceInfo.AvailableFirmware;

    public bool CanBeUpdated => DeviceInfo.CanBeUpdated;

    public DeviceFirmwareUpdateState FirmwareUpdateState => DeviceInfo.FirmwareUpdateState;

    public IReadOnlyList<ICcuDeviceChannel> Channels => _channels;

    public string Interface => DeviceInfo.Interface;

    public RxMode RxMode => DeviceInfo.RxMode;

    public override bool IsDevice => true;
}