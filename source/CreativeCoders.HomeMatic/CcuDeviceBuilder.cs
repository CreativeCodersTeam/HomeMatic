using System.Linq.Expressions;
using System.Reflection;
using CreativeCoders.HomeMatic.Abstractions.Devices;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

// public class CcuDeviceBuilder
// {
//     private CcuDeviceUri? _uri;
//
//     private DeviceDescription? _deviceDescription;
//
//     private IHomeMaticXmlRpcApi? _api;
//
//     private IEnumerable<DeviceDescription>? _devices;
//
//     public CcuDeviceBuilder WithUri(CcuDeviceUri deviceUri)
//     {
//         _uri = deviceUri;
//
//         var builder = new CcuDeviceBuilder
//         {
//             _api = _api,
//             _deviceDescription = _deviceDescription,
//             _uri = deviceUri,
//             _devices = _devices
//         };
//
//         return builder;
//     }
//
//     public CcuDeviceBuilder WithApi(IHomeMaticXmlRpcApi api)
//     {
//         _api = api;
//
//         return this;
//     }
//
//     public CcuDeviceBuilder FromDeviceDescription(DeviceDescription deviceDescription)
//     {
//         _deviceDescription = deviceDescription;
//
//         return this;
//     }
//
//     public CcuDevice Build()
//     {
//         if (_uri == null || _api == null || _devices == null)
//         {
//             throw new InvalidOperationException("Uri, Api and Devices must be set");
//         }
//
//         var ccuDevice = new CcuDevice(_api)
//         {
//             Uri = _uri,
//             DeviceType = _deviceDescription?.DeviceType ?? string.Empty,
//             Version = _deviceDescription?.Version ?? 0,
//             IsAesActive = _deviceDescription?.IsAesActive ?? false,
//             Interface = _deviceDescription?.Interface ?? string.Empty,
//             RxMode = _deviceDescription?.RxMode ?? RxMode.None,
//             RfAddress = _deviceDescription?.RfAddress ?? 0,
//             Firmware = _deviceDescription?.Firmware ?? string.Empty,
//             AvailableFirmware = _deviceDescription?.AvailableFirmware ?? string.Empty,
//             CanBeUpdated = _deviceDescription?.CanBeUpdated ?? false,
//             FirmwareUpdateState = _deviceDescription?.FirmwareUpdateState ?? DeviceFirmwareUpdateState.None,
//             Roaming = _deviceDescription?.Roaming ?? false,
//             ParamSets = _deviceDescription?.ParamSets ?? [],
//             Channels = CreateChannelsForDevice(_deviceDescription, _devices),
//         };
//
//         return ccuDevice;
//     }
//
//     private IEnumerable<ICcuDeviceChannel> CreateChannelsForDevice(DeviceDescription? deviceDescription,
//         IEnumerable<DeviceDescription> devices)
//     {
//         if (deviceDescription == null)
//         {
//             return [];
//         }
//
//         var channels = devices
//             .Where(x => x.Parent?.Equals(deviceDescription.Address, StringComparison.OrdinalIgnoreCase) ?? false)
//             .Select(x => new CcuDeviceChannel(_api!)
//             {
//                 Uri = new CcuDeviceUri
//                 {
//                     CcuHost = _uri!.CcuHost,
//                     CcuName = _uri.CcuName,
//                     Address = x.Address,
//                     Kind = _uri.Kind
//                 },
//                 DeviceType = x.DeviceType,
//                 Version = x.Version,
//                 IsAesActive = x.IsAesActive,
//                 Interface = x.Interface,
//                 Roaming = x.Roaming,
//                 ParamSets = x.ParamSets,
//                 Index = x.Index,
//                 Group = x.Group,
//                 ChannelDirection = x.ChannelDirection
//             })
//             .OrderBy(x => x.Index);
//
//         return [..channels];
//     }
//
//     public CcuDeviceBuilder WithAllDevices(IEnumerable<DeviceDescription> devices)
//     {
//         _devices = devices;
//
//         return this;
//     }
// }

public class CcuDeviceBuilder : ObjectBuilderBase<CcuDeviceBuilder, CcuDevice>
{
    private CcuDeviceUri? _uri;

    private DeviceDescription? _deviceDescription;

    private IHomeMaticXmlRpcApi? _api;

    private IEnumerable<DeviceDescription>? _devices;

    public CcuDeviceBuilder WithUri(CcuDeviceUri deviceUri) =>
        WithField(x => x._uri, deviceUri);

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

    public override CcuDevice Build()
    {
        if (_uri == null || _api == null || _devices == null)
        {
            throw new InvalidOperationException("Uri, Api and Devices must be set");
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
            ParamSets = _deviceDescription?.ParamSets ?? [],
            Channels = CreateChannelsForDevice(_deviceDescription, _devices),
        };

        return ccuDevice;
    }

    private IEnumerable<ICcuDeviceChannel> CreateChannelsForDevice(DeviceDescription? deviceDescription,
        IEnumerable<DeviceDescription> devices)
    {
        if (deviceDescription == null)
        {
            return [];
        }

        var channels = devices
            .Where(x => x.Parent?.Equals(deviceDescription.Address, StringComparison.OrdinalIgnoreCase) ?? false)
            .Select(x => new CcuDeviceChannel(_api!)
            {
                Uri = new CcuDeviceUri
                {
                    CcuHost = _uri!.CcuHost,
                    CcuName = _uri.CcuName,
                    Address = x.Address,
                    Kind = _uri.Kind
                },
                DeviceType = x.DeviceType,
                Version = x.Version,
                IsAesActive = x.IsAesActive,
                Interface = x.Interface,
                Roaming = x.Roaming,
                ParamSets = x.ParamSets,
                Index = x.Index,
                Group = x.Group,
                ChannelDirection = x.ChannelDirection
            })
            .OrderBy(x => x.Index);

        return [..channels];
    }

    public CcuDeviceBuilder WithAllDevices(IEnumerable<DeviceDescription> devices)
    {
        _devices = devices;

        return this;
    }
}

public abstract class ObjectBuilderBase<TBuilderImpl, TOutput>
    where TBuilderImpl : class, new()
{
    protected TBuilderImpl WithField<TProperty>(Expression<Func<TBuilderImpl, TProperty>> property, TProperty value)
    {
        var member = (MemberExpression)property.Body;
        var targetFieldInfo = (FieldInfo)member.Member;

        var obj = new TBuilderImpl();

        foreach (var fieldInfo in typeof(TBuilderImpl).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            fieldInfo.SetValue(obj, fieldInfo.GetValue(this));
        }

        targetFieldInfo.SetValue(obj, value);

        return obj;
    }

    public abstract TOutput Build();
}
