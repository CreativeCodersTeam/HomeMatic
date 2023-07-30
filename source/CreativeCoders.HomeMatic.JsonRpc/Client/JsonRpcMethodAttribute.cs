namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public class JsonRpcMethodAttribute : Attribute
{
    public JsonRpcMethodAttribute(string methodName)
    {
        MethodName = methodName;
    }
    
    public string MethodName { get; }
}
