namespace CreativeCoders.HomeMatic;

public interface ICcuDeviceChannel
{
    CcuDeviceUri Uri { get; }
}

public class CcuDeviceChannel : ICcuDeviceChannel
{
    public required CcuDeviceUri Uri { get; init; }
}
