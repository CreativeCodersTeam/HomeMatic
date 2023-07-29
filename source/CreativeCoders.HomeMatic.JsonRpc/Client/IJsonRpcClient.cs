namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public interface IJsonRpcClient
{
    Task<JsonRpcResponse> ExecuteAsync(Uri url, string methodName, params object[] arguments);
}