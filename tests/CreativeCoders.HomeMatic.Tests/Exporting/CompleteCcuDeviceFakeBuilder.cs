using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.Tests.Exporting;

internal sealed class CompleteCcuDeviceFakeBuilder
{
    private string _name = "TestDevice";
    private string _address = "ABC123456";
    private string _deviceType = "HM-CC-RT-DN";
    private string _firmware = "1.4";
    private string _ccuHost = "ccu2.local";
    private string _ccuName = string.Empty;
    private string[] _paramSetKeys = ["MASTER", "VALUES"];
    private readonly List<ParamSetValuesWithDescriptions> _paramSetValues = [];
    private readonly List<ICompleteCcuDeviceChannel> _channels = [];

    public CompleteCcuDeviceFakeBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithDeviceType(string deviceType)
    {
        _deviceType = deviceType;
        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithFirmware(string firmware)
    {
        _firmware = firmware;
        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithCcuHost(string ccuHost)
    {
        _ccuHost = ccuHost;
        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithCcuName(string ccuName)
    {
        _ccuName = ccuName;
        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithParamSetKeys(params string[] paramSetKeys)
    {
        _paramSetKeys = paramSetKeys;
        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithParamSet(string paramSetKey, Action<ParamSetValuesBuilder>? configure = null)
    {
        var builder = new ParamSetValuesBuilder();
        configure?.Invoke(builder);

        _paramSetValues.Add(new ParamSetValuesWithDescriptions
        {
            ParamSetKey = paramSetKey,
            ParamSetValues = builder.Build().ToList()
        });

        return this;
    }

    public CompleteCcuDeviceFakeBuilder WithChannel(Action<CompleteCcuDeviceChannelFakeBuilder>? configure = null)
    {
        var builder = new CompleteCcuDeviceChannelFakeBuilder().WithCcuHost(_ccuHost);
        configure?.Invoke(builder);
        _channels.Add(builder.Build());
        return this;
    }

    public ICompleteCcuDevice Build()
    {
        var device = A.Fake<ICompleteCcuDevice>();
        var deviceData = A.Fake<ICcuDeviceData>();

        var uri = new CcuDeviceUri
        {
            CcuHost = _ccuHost,
            CcuName = _ccuName,
            Kind = CcuDeviceKind.HomeMatic,
            Address = _address
        };

        A.CallTo(() => deviceData.Name).Returns(_name);
        A.CallTo(() => deviceData.Uri).Returns(uri);
        A.CallTo(() => deviceData.DeviceType).Returns(_deviceType);
        A.CallTo(() => deviceData.Firmware).Returns(_firmware);
        A.CallTo(() => deviceData.ParamSets).Returns(_paramSetKeys);

        A.CallTo(() => device.DeviceData).Returns(deviceData);
        A.CallTo(() => device.ParamSetValues).Returns(_paramSetValues);
        A.CallTo(() => device.Channels).Returns(_channels);

        return device;
    }
}
