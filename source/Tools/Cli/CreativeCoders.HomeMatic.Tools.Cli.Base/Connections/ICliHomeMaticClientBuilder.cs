using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public interface ICliHomeMaticClientBuilder
{
    Task<IMultiCcuClient> BuildMultiCcuClientAsync();
}
