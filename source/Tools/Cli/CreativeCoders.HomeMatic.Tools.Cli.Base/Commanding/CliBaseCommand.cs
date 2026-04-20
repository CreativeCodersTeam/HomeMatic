using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class CliBaseCommand
{
    private readonly IHomeMaticXmlRpcApiBuilder _apiBuilder;

    protected CliBaseCommand(IHomeMaticXmlRpcApiBuilder apiBuilder, ISharedData sharedData)
    {
        _apiBuilder = Ensure.NotNull(apiBuilder);
        SharedData = Ensure.NotNull(sharedData);
    }
    
    protected IHomeMaticXmlRpcApi BuildApi()
    {
        var cliData = SharedData.LoadCliData();
        
        return _apiBuilder
            .ForUrl(new Uri($"http://{cliData.CcuHost}:{CcuRpcPorts.HomeMaticIp}"))
            .Build();
    }

    protected ISharedData SharedData { get; set; }
}
