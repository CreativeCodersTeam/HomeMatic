namespace CreativeCoders.HomeMatic.Client.Core;

public interface IHomeMaticClientBuilder
{
    IHomeMaticClientBuilder AddCcu(HomeMaticCcuConnectionInfo ccuConnectionInfo);
    
    IHomeMaticClient Build();
}