namespace CreativeCoders.HomeMatic.Api.Core
{
    public interface ICcuConnectionBuilder
    {
        ICcuConnectionBuilder ForAddress(string ccuAddress);
        
        ICcuConnection Build();
    }
}