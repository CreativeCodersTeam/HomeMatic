namespace CreativeCoders.HomeMatic.Client.Core;

public interface IHomeMaticClientBuilder
{
    IHomeMaticClientBuilder AddCcu(HomeMaticCcu ccu);
    
    IHomeMaticClient Build();
}