namespace CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;

public interface IJsonRpcApiBuilder<T>
    where T : class
{
    IJsonRpcApiBuilder<T> ForUrl(Uri url);
    
    T Build();
}
