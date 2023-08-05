namespace CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;

[AttributeUsage(AttributeTargets.Interface)]
public class JsonRpcApiAttribute : Attribute
{
    public bool IncludeParameterNames { get; set; }
}
