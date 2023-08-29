using CreativeCoders.Core;
using CreativeCoders.Net.JsonRpc.ApiBuilder;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcApiBuilder
{
    IHomeMaticJsonRpcApiBuilder ForUrl(Uri url);

    IHomeMaticJsonRpcApi Build();
}

public class HomeMaticJsonRpcApiBuilder : IHomeMaticJsonRpcApiBuilder
{
    private readonly IJsonRpcApiBuilder<IHomeMaticJsonRpcApi> _apiProxyBuilder;
    
    private Uri? _url;

    public HomeMaticJsonRpcApiBuilder(IJsonRpcApiBuilder<IHomeMaticJsonRpcApi> apiProxyBuilder)
    {
        _apiProxyBuilder = Ensure.NotNull(apiProxyBuilder);
    }
    
    public IHomeMaticJsonRpcApiBuilder ForUrl(Uri url)
    {
        _url = url;

        return this;
    }

    public IHomeMaticJsonRpcApi Build()
    {
        if (_url == null)
        {
            throw new InvalidOperationException("No url specified");
        }

        return _apiProxyBuilder
            .ForUrl(_url)
            .Build();
    }
}
