using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class CliBaseCommand(IHomeMaticXmlRpcApiBuilder apiBuilder, ISharedData sharedData)
{
    private readonly IHomeMaticXmlRpcApiBuilder _apiBuilder = Ensure.NotNull(apiBuilder);

    protected IHomeMaticXmlRpcApi BuildApi()
    {
        var cliData = SharedData.LoadCliData();

        return _apiBuilder
            .ForUrl(new Uri($"http://{cliData.CcuHost}:{CcuRpcPorts.HomeMaticIp}"))
            .Build();
    }

    protected ISharedData SharedData { get; } = Ensure.NotNull(sharedData);
}
