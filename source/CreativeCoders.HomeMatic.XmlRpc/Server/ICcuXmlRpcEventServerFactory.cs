using CreativeCoders.Net.XmlRpc.Server;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

public interface ICcuXmlRpcEventServerFactory
{
    ICcuXmlRpcEventServer CreateServer();
    
    ICcuXmlRpcEventServer CreateServer(IXmlRpcServer xmlRpcServer);
}