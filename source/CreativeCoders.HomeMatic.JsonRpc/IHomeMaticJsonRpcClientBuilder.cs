using System.Net;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcClientBuilder
{
    IHomeMaticJsonRpcClientBuilder ForUrl(Uri url);
    
    IHomeMaticJsonRpcClientBuilder WithCredentials(NetworkCredential credential);
    
    IHomeMaticJsonRpcClient Build();
}

public class HomeMaticJsonRpcClientBuilder : IHomeMaticJsonRpcClientBuilder
{
    private readonly IHomeMaticJsonRpcApiBuilder _apiBuilder;
    
    private Uri? _url;

    private NetworkCredential? _credential;

    public HomeMaticJsonRpcClientBuilder(IHomeMaticJsonRpcApiBuilder apiBuilder)
    {
        _apiBuilder = Ensure.NotNull(apiBuilder);
    }
    
    public IHomeMaticJsonRpcClientBuilder ForUrl(Uri url)
    {
        _url = url;

        return this;
    }

    public IHomeMaticJsonRpcClientBuilder WithCredentials(NetworkCredential credential)
    {
        _credential = Ensure.NotNull(credential);

        return this;
    }

    public IHomeMaticJsonRpcClient Build()
    {
        if (_url == null)
        {
            throw new InvalidOperationException("No url specified");
        }
        
        return new HomeMaticJsonRpcClient(_apiBuilder.ForUrl(_url).Build())
        {
            Credential = _credential
        };
    }
}
