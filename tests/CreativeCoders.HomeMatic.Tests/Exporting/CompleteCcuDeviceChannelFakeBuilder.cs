using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.Tests.Exporting;

internal sealed class CompleteCcuDeviceChannelFakeBuilder
{
    private string _address = "ABC123456:1";
    private string _deviceType = "HM-CC-RT-DN:01";
    private int _index = 1;
    private string[] _paramSets = ["VALUES"];
    private readonly List<ParamSetValuesWithDescriptions> _paramSetValues = [];
    private string _ccuHost = "ccu2.local";

    public CompleteCcuDeviceChannelFakeBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public CompleteCcuDeviceChannelFakeBuilder WithDeviceType(string deviceType)
    {
        _deviceType = deviceType;
        return this;
    }

    public CompleteCcuDeviceChannelFakeBuilder WithIndex(int index)
    {
        _index = index;
        return this;
    }

    public CompleteCcuDeviceChannelFakeBuilder WithParamSets(params string[] paramSets)
    {
        _paramSets = paramSets;
        return this;
    }

    public CompleteCcuDeviceChannelFakeBuilder WithCcuHost(string ccuHost)
    {
        _ccuHost = ccuHost;
        return this;
    }

    public CompleteCcuDeviceChannelFakeBuilder WithParamSet(string paramSetKey, Action<ParamSetValuesBuilder>? configure = null)
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

    public ICompleteCcuDeviceChannel Build()
    {
        var channel = A.Fake<ICompleteCcuDeviceChannel>();
        var channelData = A.Fake<ICcuDeviceChannelData>();

        var uri = new CcuDeviceUri
        {
            CcuHost = _ccuHost,
            Kind = CcuDeviceKind.HomeMatic,
            Address = _address
        };

        A.CallTo(() => channelData.Uri).Returns(uri);
        A.CallTo(() => channelData.DeviceType).Returns(_deviceType);
        A.CallTo(() => channelData.Index).Returns(_index);
        A.CallTo(() => channelData.ParamSets).Returns(_paramSets);

        A.CallTo(() => channel.ChannelData).Returns(channelData);
        A.CallTo(() => channel.ParamSetValues).Returns(_paramSetValues);

        return channel;
    }
}
