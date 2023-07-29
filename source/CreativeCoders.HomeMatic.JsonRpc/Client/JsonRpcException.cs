namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public class JsonRpcException : Exception
{
    public JsonRpcException(int errorCode, string? errorMessage)
        : base($"Json RPC error ({errorCode}): {errorMessage}")
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    
    public int ErrorCode { get; }
    
    public string? ErrorMessage { get; }
}
