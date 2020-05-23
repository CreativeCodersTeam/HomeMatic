namespace CreativeCoders.HomeMatic.Core.Values
{
    public interface ICcuValueAddress
    {
        string DeviceAddress { get; }
        
        string ValueKey { get; }
    }
}