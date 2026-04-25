using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic;

/// <inheritdoc />
/// <summary>
/// Default implementation of <see cref="T:CreativeCoders.HomeMatic.Core.IMultiCcuClientFactory">IMultiCcuClientFactory</see> that collects CCU configurations and
/// builds an <see cref="T:CreativeCoders.HomeMatic.Core.IMultiCcuClient">IMultiCcuClient</see> backed by a <see cref="T:CreativeCoders.HomeMatic.CcuRoutingTable">CcuRoutingTable</see>.
/// </summary>
/// <param name="ccuClientFactory">The factory used to create the per-CCU <see cref="T:CreativeCoders.HomeMatic.Core.ICcuClient">ICcuClient</see> instances.</param>
public class MultiCcuClientFactory(ICcuClientFactory ccuClientFactory) : IMultiCcuClientFactory
{
    private readonly List<ICcuClient> _ccuClients = [];

    /// <inheritdoc />
    public IMultiCcuClientFactory AddCcu(string ccuName, string host, string userName, string password,
        params CcuDeviceKind[] deviceKinds)
    {
        var ccuClient = ccuClientFactory.CreateClient(ccuName, deviceKinds, host, userName, password);

        _ccuClients.Add(ccuClient);

        return this;
    }

    /// <inheritdoc />
    public IMultiCcuClient Build()
    {
        return new MultiCcuClient(_ccuClients, new CcuRoutingTable());
    }
}
