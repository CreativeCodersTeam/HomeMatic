using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic;

public class MultiCcuClientFactory(ICcuClientFactory ccuClientFactory) : IMultiCcuClientFactory
{
    private readonly List<ICcuClient> _ccuClients = [];

    public IMultiCcuClientFactory AddCcu(string ccuName, string host, string userName, string password,
        params CcuDeviceKind[] deviceKinds)
    {
        var ccuClient = ccuClientFactory.CreateClient(ccuName, deviceKinds, host, userName, password);

        _ccuClients.Add(ccuClient);

        return this;
    }

    public IMultiCcuClient Build()
    {
        return new MultiCcuClient(_ccuClients);
    }
}
