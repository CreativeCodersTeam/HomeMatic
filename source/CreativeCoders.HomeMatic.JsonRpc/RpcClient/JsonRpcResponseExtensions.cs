namespace CreativeCoders.HomeMatic.JsonRpc.RpcClient;

public static class JsonRpcResponseExtensions
{
    public static void EnsureSuccess<T>(this JsonRpcResponse<T> response, string methodName)
    {
        if (response.Error != null)
        {
            throw new JsonRpcCallException(response.Error.Code, response.Error.Message, methodName);
        }
    }
}
