namespace CreativeCoders.HomeMatic.Core
{
    public interface ICcuBuilder
    {
        ICcuBuilder ForAddress(string ccuAddress);
        
        ICcu Build();
    }
}