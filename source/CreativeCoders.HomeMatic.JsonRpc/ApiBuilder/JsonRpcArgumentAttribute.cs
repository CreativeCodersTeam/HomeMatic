namespace CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;

[AttributeUsage(AttributeTargets.Parameter)]
public class JsonRpcArgumentAttribute : Attribute
{
    public JsonRpcArgumentAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
