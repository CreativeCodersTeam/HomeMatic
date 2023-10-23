using System.Net;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcClientBuilder
{
    IHomeMaticJsonRpcClientBuilder ForUrl(Uri url);
    
    IHomeMaticJsonRpcClientBuilder WithCredentials(NetworkCredential credential);
    
    IHomeMaticJsonRpcClient Build();
}