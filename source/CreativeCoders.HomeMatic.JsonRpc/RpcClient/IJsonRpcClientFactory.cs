namespace CreativeCoders.HomeMatic.JsonRpc.RpcClient;

public interface IJsonRpcClientFactory
{
    IJsonRpcClient Create(string name);
    
    IJsonRpcClient Create();
}