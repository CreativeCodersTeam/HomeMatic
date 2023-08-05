namespace CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;

[AttributeUsage(AttributeTargets.Method)]
public class JsonRpcMethodAttribute : Attribute
{
    public JsonRpcMethodAttribute(string methodName)
    {
        MethodName = methodName;
    }
    
    public string MethodName { get; }
}
