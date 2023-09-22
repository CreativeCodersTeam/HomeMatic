using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticClientBuilder : IHomeMaticClientBuilder
{
    private readonly IHomeMaticXmlRpcApiBuilder _xmlRpcApiBuilder;
    
    private readonly IHomeMaticJsonRpcApiBuilder _jsonRpcApiBuilder;
    
    private List<HomeMaticCcu> _ccus = new List<HomeMaticCcu>();
    
    public HomeMaticClientBuilder(IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
        IHomeMaticJsonRpcApiBuilder jsonRpcApiBuilder)
    {
        _xmlRpcApiBuilder = Ensure.NotNull(xmlRpcApiBuilder);
        _jsonRpcApiBuilder = Ensure.NotNull(jsonRpcApiBuilder);
    }
    
    public IHomeMaticClientBuilder AddCcu(HomeMaticCcu ccu)
    {
        _ccus.Add(ccu);
        
        return this;
    }

    private IEnumerable<HomeMaticCcuConnection> CreateConnection(HomeMaticCcu ccu)
    {
        yield break;
        // var xmlRpcApi = _xmlRpcApiBuilder.ForUrl()
        // return new HomeMaticCcuConnection(ccu)
    }

    public IHomeMaticClient Build()
    {
        return new HomeMaticClient(_ccus.SelectMany(CreateConnection).ToArray());
    }
}
