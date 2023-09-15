namespace CreativeCoders.HomeMatic.JsonRpc.Api;

public interface IHomeMaticJsonRpcApiBuilder
{
    IHomeMaticJsonRpcApiBuilder ForUrl(Uri url);

    IHomeMaticJsonRpcApi Build();
}