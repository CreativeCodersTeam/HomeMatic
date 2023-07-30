namespace CreativeCoders.HomeMatic.JsonRpc;

public class DeviceDetails
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Interface { get; set; }

    public string? Type { get; set; }

    public bool OperateGroupOnly { get; set; }

    public bool IsReady { get; set; }
}
