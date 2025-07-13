using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Client.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public interface ICliHomeMaticClientBuilder
{
    Task<IHomeMaticClient> BuildAsync();

    Task<IMultiCcuClient> BuildMultiCcuClientAsync();
}
