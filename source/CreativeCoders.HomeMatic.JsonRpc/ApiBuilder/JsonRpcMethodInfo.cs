namespace CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;

public class JsonRpcMethodInfo
{
    public string?[] ArgumentNames { get; init; } = Array.Empty<string>();
    
    public string RpcMethodName { get; init; } = string.Empty;

    public bool HasCompleteJsonResponse { get; init; }

    public Type? ResultType { get; init; }
}
