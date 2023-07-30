namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public interface IJsonRpcClient
{
    Task<JsonRpcResponse<T>> ExecuteAsync<T>(Uri url, string methodName, params object[] arguments);
}