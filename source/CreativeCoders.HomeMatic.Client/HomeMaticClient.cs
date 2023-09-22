using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.Client.Core.Devices;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticClient : IHomeMaticClient
{
    public HomeMaticClient(HomeMaticCcuConnection[] connections)
    {
        
    }
    
    public Task<CcuDevice> GetDevicesAsync()
    {
        throw new NotImplementedException();
    }
}
