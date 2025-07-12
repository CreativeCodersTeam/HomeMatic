using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic;

public class CcuClient(
    IHomeMaticJsonRpcClient jsonRpcClient,
    IDictionary<CcuDeviceKind, IHomeMaticXmlRpcApi> xmlRpcApis) : ICcuClient
{
    public Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICcuDevice> GetDeviceAsync(string address)
    {
        throw new NotImplementedException();
    }
}
