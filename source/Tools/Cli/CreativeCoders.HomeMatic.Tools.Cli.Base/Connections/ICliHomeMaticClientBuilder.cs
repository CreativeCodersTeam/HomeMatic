using CreativeCoders.HomeMatic.Abstractions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public interface ICliHomeMaticClientBuilder
{
    Task<IMultiCcuClient> BuildMultiCcuClientAsync();
}
