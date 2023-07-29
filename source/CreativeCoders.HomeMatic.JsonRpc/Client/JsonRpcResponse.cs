namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public class JsonRpcResponse
{
    public int Id { get; set; }

    public object? Result { get; set; }

    public JsonRpcError? Error { get; set; }
}