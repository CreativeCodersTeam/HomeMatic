using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public abstract class CliBaseCommand
{
    private readonly IHomeMaticXmlRpcApiBuilder _apiBuilder;

    protected CliBaseCommand(IHomeMaticXmlRpcApiBuilder apiBuilder, ISharedData sharedData)
    {
        _apiBuilder = Ensure.NotNull(apiBuilder, nameof(apiBuilder));
        SharedData = Ensure.NotNull(sharedData, nameof(sharedData));
    }
    
    protected IHomeMaticXmlRpcApi BuildApi()
    {
        var cliData = SharedData.LoadCliData();
        
        return _apiBuilder
            .ForUrl($"http://{cliData.CcuHost}:{CcuRpcPorts.HomeMaticIp}")
            .Build();
    }

    protected ISharedData SharedData { get; set; }
}
