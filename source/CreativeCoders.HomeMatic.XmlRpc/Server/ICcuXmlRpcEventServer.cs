using System.Threading.Tasks;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

public interface ICcuXmlRpcEventServer
{
    Task StartAsync();

    Task StopAsync();

    void RegisterEventHandler(ICcuEventHandler eventHandler);

    string ServerUrl { get; set; }
}