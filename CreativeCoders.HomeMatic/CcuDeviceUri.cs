using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic;

public class CcuDeviceUri
{
    public required string Host { get; init; }

    public required CcuDeviceKind Kind { get; init; }

    public required string Address { get; init; }

    public override string ToString()
    {
        return $"{Kind}://{Host}/{Address}";
    }
}
