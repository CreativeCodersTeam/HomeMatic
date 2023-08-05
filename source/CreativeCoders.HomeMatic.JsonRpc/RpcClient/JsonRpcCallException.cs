namespace CreativeCoders.HomeMatic.JsonRpc.RpcClient;

public class JsonRpcCallException : Exception
{
    public JsonRpcCallException(int errorCode, string? errorMessage, string methodName)
        : base($"Json RPC method '{methodName}' call failed ({errorCode}): {errorMessage}")
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    
    public int ErrorCode { get; }
    
    public string? ErrorMessage { get; }
}
