using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic;

public class CcuDeviceUri
{
    public required string Host { get; init; }

    public required HomeMaticDeviceSystems DeviceSystems { get; init; }

    public required string Address { get; init; }

    public override string ToString()
    {
        return $"{DeviceSystems}://{Host}/{Address}";
    }
}
