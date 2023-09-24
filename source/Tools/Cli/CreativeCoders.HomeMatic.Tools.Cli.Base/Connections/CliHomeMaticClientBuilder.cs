using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Client.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CliHomeMaticClientBuilder : ICliHomeMaticClientBuilder
{
    private readonly ICcuConnectionsStore _ccuConnectionsStore;
    
    private readonly IHomeMaticClientBuilder _homeMaticClientBuilder;

    public CliHomeMaticClientBuilder(ICcuConnectionsStore ccuConnectionsStore, IHomeMaticClientBuilder homeMaticClientBuilder)
    {
        _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);
        _homeMaticClientBuilder = Ensure.NotNull(homeMaticClientBuilder);
    }
    
    public async Task<IHomeMaticClient> BuildAsync()
    {
        var connections = await _ccuConnectionsStore.GetConnectionsAsync()
            .ConfigureAwait(false);
        
        connections.ForEach(x =>
        {
            var credential = _ccuConnectionsStore.GetCredentials(x);
            
            _homeMaticClientBuilder.AddCcu(
                new HomeMaticCcu(x.Name, x.Url)
                {
                    Systems =
                        HomeMaticSystem.HomeMatic | HomeMaticSystem.HomeMaticIp | HomeMaticSystem.HomeMaticIpWired,
                    Username = credential.UserName,
                    Password = credential.Password
                });
        });

        return _homeMaticClientBuilder.Build();
    }
}
