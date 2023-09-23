using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticClientBuilder : IHomeMaticClientBuilder
{
    private readonly IHomeMaticXmlRpcApiBuilder _xmlRpcApiBuilder;
    
    private readonly IHomeMaticJsonRpcApiBuilder _jsonRpcApiBuilder;
    
    private List<HomeMaticCcu> _ccus = new List<HomeMaticCcu>();
    
    public HomeMaticClientBuilder(IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
        IHomeMaticJsonRpcApiBuilder jsonRpcApiBuilder)
    {
        _xmlRpcApiBuilder = Ensure.NotNull(xmlRpcApiBuilder);
        _jsonRpcApiBuilder = Ensure.NotNull(jsonRpcApiBuilder);
    }
    
    public IHomeMaticClientBuilder AddCcu(HomeMaticCcu ccu)
    {
        _ccus.Add(ccu);
        
        return this;
    }

    private HomeMaticCcuConnection CreateConnection(HomeMaticCcu ccu)
    {
        var jsonRpcApi = _jsonRpcApiBuilder
            .ForUrl(ccu.Url)
            .Build();

        var xmlRpcApis = ccu.Systems.Enumerate().Select(x =>
            {
                var url = new Uri($"{ccu.Url}:{SystemToPort(x)}");

                var xmlRpcApi = _xmlRpcApiBuilder
                    .ForUrl(url)
                    .Build();

                return new XmlRpcApi(x, xmlRpcApi);
            })
            .ToArray();

        return new HomeMaticCcuConnection(ccu, xmlRpcApis, jsonRpcApi);
    }

    private int SystemToPort(HomeMaticSystem system)
    {
        return system switch
        {
            HomeMaticSystem.HomeMatic => CcuRpcPorts.HomeMatic,
            HomeMaticSystem.HomeMaticIp => CcuRpcPorts.HomeMaticIp,
            HomeMaticSystem.HomeMaticIpWired => CcuRpcPorts.HomeMaticWired,
            _ => throw new ArgumentOutOfRangeException(nameof(system), system, null)
        };
    }

    public IHomeMaticClient Build()
    {
        return new HomeMaticClient(_ccus.Select(CreateConnection).ToArray());
    }
}

public static class EnumExtensions
{
    public static IEnumerable<T> Enumerate<T>(this T flags)
        where T : struct, Enum
    {
        return Enum
            .GetValues<T>()
            .Where(x => flags.HasFlag(x));
    }
}
