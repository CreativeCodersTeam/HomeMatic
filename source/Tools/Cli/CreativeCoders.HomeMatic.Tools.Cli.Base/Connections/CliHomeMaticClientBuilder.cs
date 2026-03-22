using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CliHomeMaticClientBuilder(
    ICcuConnectionsStore ccuConnectionsStore,
    IMultiCcuClientFactory multiCcuClientFactory)
    : ICliHomeMaticClientBuilder
{
    private readonly IMultiCcuClientFactory _multiCcuClientFactory = Ensure.NotNull(multiCcuClientFactory);

    private readonly ICcuConnectionsStore _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);

    public async Task<IMultiCcuClient> BuildMultiCcuClientAsync()
    {
        var connections = await _ccuConnectionsStore.GetConnectionsAsync()
            .ConfigureAwait(false);

        connections.ForEach(x =>
        {
            var credential = _ccuConnectionsStore.GetCredentials(x);

            _multiCcuClientFactory.AddCcu(x.Name, x.Url.Host, credential.UserName, credential.Password,
                [CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp]);
        });

        return _multiCcuClientFactory.Build();
    }

    public IMultiCcuClient BuildMultiCcuClient()
    {
        var connections = _ccuConnectionsStore.GetConnections();

        connections.ForEach(x =>
        {
            var credential = _ccuConnectionsStore.GetCredentials(x);

            _multiCcuClientFactory.AddCcu(x.Name, x.Url.Host, credential.UserName, credential.Password,
                [CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp]);
        });

        return _multiCcuClientFactory.Build();
    }
}
