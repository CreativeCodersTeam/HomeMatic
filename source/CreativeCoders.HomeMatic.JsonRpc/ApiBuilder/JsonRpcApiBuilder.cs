using CreativeCoders.Core;
using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.HomeMatic.JsonRpc.RpcClient;

namespace CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;

public class JsonRpcApiBuilder<T> : IJsonRpcApiBuilder<T>
    where T : class
{
    private readonly IProxyBuilder<T> _proxyBuilder;
    
    private readonly IJsonRpcClientFactory _jsonRpcClientFactory;

    private Uri? _url;

    public JsonRpcApiBuilder(IProxyBuilder<T> proxyBuilder, IJsonRpcClientFactory jsonRpcClientFactory)
    {
        _proxyBuilder = Ensure.NotNull(proxyBuilder, nameof(proxyBuilder));
        _jsonRpcClientFactory = Ensure.NotNull(jsonRpcClientFactory, nameof(jsonRpcClientFactory));
    }
    
    public IJsonRpcApiBuilder<T> ForUrl(Uri url)
    {
        _url = url;

        return this;
    }

    public T Build()
    {
        if (_url == null)
        {
            throw new InvalidOperationException("No url set");
        }

        var api = _proxyBuilder.Build(new JsonRpcApiInterceptor<T>(_jsonRpcClientFactory.Create(), _url));

        return api;
    }
}