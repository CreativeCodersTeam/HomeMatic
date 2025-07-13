using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CliHomeMaticClientBuilder : ICliHomeMaticClientBuilder
{
    private readonly IMultiCcuClientFactory _multiCcuClientFactory;

    private readonly ICcuConnectionsStore _ccuConnectionsStore;

    private readonly IHomeMaticClientBuilder _homeMaticClientBuilder;

    public CliHomeMaticClientBuilder(ICcuConnectionsStore ccuConnectionsStore,
        IHomeMaticClientBuilder homeMaticClientBuilder,
        IMultiCcuClientFactory multiCcuClientFactory)
    {
        _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);
        _homeMaticClientBuilder = Ensure.NotNull(homeMaticClientBuilder);
        _multiCcuClientFactory = Ensure.NotNull(multiCcuClientFactory);
    }

    public async Task<IHomeMaticClient> BuildAsync()
    {
        var connections = await _ccuConnectionsStore.GetConnectionsAsync()
            .ConfigureAwait(false);

        connections.ForEach(x =>
        {
            var credential = _ccuConnectionsStore.GetCredentials(x);

            _homeMaticClientBuilder.AddCcu(
                new HomeMaticCcuConnectionInfo(x.Name, x.Url)
                {
                    Systems =
                        HomeMaticDeviceSystems.HomeMatic | HomeMaticDeviceSystems.HomeMaticIp,
                    Username = credential.UserName,
                    Password = credential.Password
                });
        });

        return _homeMaticClientBuilder.Build();
    }

    public async Task<IMultiCcuClient> BuildMultiCcuClientAsync()
    {
        var connections = await _ccuConnectionsStore.GetConnectionsAsync()
            .ConfigureAwait(false);

        connections.ForEach(x =>
        {
            var credential = _ccuConnectionsStore.GetCredentials(x);

            _multiCcuClientFactory.AddCcu(x.Url.Host, credential.UserName, credential.Password,
                [CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp]);
        });

        return _multiCcuClientFactory.Build();
    }
}
