using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.JsonRpc.RpcClient;

public class JsonRpcClientFactory : IJsonRpcClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public JsonRpcClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = Ensure.NotNull(httpClientFactory, nameof(httpClientFactory));
    }
    
    public IJsonRpcClient Create(string name)
    {
        return new JsonRpcClient(_httpClientFactory.CreateClient(name));
    }

    public IJsonRpcClient Create()
    {
        return new JsonRpcClient(_httpClientFactory.CreateClient());
    }
}
