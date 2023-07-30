namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public class JsonRpcResponse<T>
{
    public int Id { get; set; }

    public T? Result { get; set; }

    public JsonRpcError? Error { get; set; }
}