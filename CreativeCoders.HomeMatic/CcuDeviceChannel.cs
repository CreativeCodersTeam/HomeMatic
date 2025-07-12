using CreativeCoders.HomeMatic.Abstractions;

namespace CreativeCoders.HomeMatic;

public class CcuDeviceChannel : ICcuDeviceChannel
{
    public required CcuDeviceUri Uri { get; init; }
}
