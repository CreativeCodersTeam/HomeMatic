using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

/// <summary>
/// Fluent builder that constructs a <see cref="CcuDevice"/> including its channels from an XML-RPC
/// <see cref="DeviceDescription"/> and the list of all devices that share the same CCU.
/// </summary>
public class CcuDeviceBuilder : ObjectBuilderBase<CcuDeviceBuilder, CcuDevice>
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [SuppressMessage("csharpsquid", "S3459",
        Justification = "Fields are set via WithField method and not directly assigned to.")]
    private CcuDeviceUri? _uri;

    private DeviceDescription? _deviceDescription;

    private IHomeMaticXmlRpcApi? _api;

    private IEnumerable<DeviceDescription>? _devices;

    /// <summary>
    /// Sets the <see cref="CcuDeviceUri"/> of the device to build.
    /// </summary>
    /// <param name="deviceUri">The URI identifying the device on its CCU.</param>
    /// <returns>A new <see cref="CcuDeviceBuilder"/> instance with the value applied.</returns>
    public CcuDeviceBuilder WithUri(CcuDeviceUri deviceUri) =>
        WithField(x => x._uri, deviceUri);

    /// <summary>
    /// Sets the XML-RPC API that the built device should use for parameter-set access.
    /// </summary>
    /// <param name="api">The XML-RPC API instance.</param>
    /// <returns>The same <see cref="CcuDeviceBuilder"/> instance, to allow chaining calls.</returns>
    public CcuDeviceBuilder WithApi(IHomeMaticXmlRpcApi api)
    {
        _api = api;

        return this;
    }

    /// <summary>
    /// Seeds the builder with a <see cref="DeviceDescription"/> whose values are copied onto the built device.
    /// </summary>
    /// <param name="deviceDescription">The device description returned by the CCU.</param>
    /// <returns>The same <see cref="CcuDeviceBuilder"/> instance, to allow chaining calls.</returns>
    public CcuDeviceBuilder FromDeviceDescription(DeviceDescription deviceDescription)
    {
        _deviceDescription = deviceDescription;

        return this;
    }

    /// <summary>
    /// Builds the <see cref="CcuDevice"/> from the previously configured values.
    /// </summary>
    /// <returns>A new <see cref="CcuDevice"/> instance populated with device and channel data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="WithUri"/>, <see cref="WithApi"/> or <see cref="WithAllDevices"/> has not been called.</exception>
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
            RxMode = _deviceDescription?.RxMode ?? RxModes.None,
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
            .Where(x => x.Parent.Equals(deviceDescription.Address, StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    /// Sets the list of all device descriptions known for the CCU so that the builder can resolve the
    /// channels that belong to the device currently being built.
    /// </summary>
    /// <param name="devices">All device descriptions of the CCU, including channels.</param>
    /// <returns>The same <see cref="CcuDeviceBuilder"/> instance, to allow chaining calls.</returns>
    public CcuDeviceBuilder WithAllDevices(IEnumerable<DeviceDescription> devices)
    {
        _devices = devices;

        return this;
    }
}

/// <summary>
/// Base class for immutable fluent builders that produce a new builder instance on every configuration step.
/// </summary>
/// <typeparam name="TBuilderImpl">The concrete builder type. Used so that <c>WithField</c> returns a new instance of the same type.</typeparam>
/// <typeparam name="TOutput">The type produced by <see cref="Build"/>.</typeparam>
public abstract class ObjectBuilderBase<TBuilderImpl, TOutput>
    where TBuilderImpl : class, new()
{
    /// <summary>
    /// Creates a new builder instance, copies all private fields from the current instance and applies the
    /// supplied value to the selected field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the field being assigned.</typeparam>
    /// <param name="property">An expression selecting the private backing field to set.</param>
    /// <param name="value">The value to assign to the selected field.</param>
    /// <returns>A new <typeparamref name="TBuilderImpl"/> instance with the updated value.</returns>
    [SuppressMessage("csharpsquid", "S3011", Justification = "Reflection only used for writing own private fields")]
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

    /// <summary>
    /// Builds the configured object.
    /// </summary>
    /// <returns>The built <typeparamref name="TOutput"/> instance.</returns>
    public abstract TOutput Build();
}
