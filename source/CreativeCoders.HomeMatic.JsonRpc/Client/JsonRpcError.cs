namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public class JsonRpcError
{
    public string? Name { get; set; }

    public int Code { get; set; }

    public string? Message { get; set; }
}