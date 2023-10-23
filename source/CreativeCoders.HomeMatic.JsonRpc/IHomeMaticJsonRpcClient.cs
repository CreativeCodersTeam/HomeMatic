using System.Net;
using CreativeCoders.HomeMatic.JsonRpc.Models;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcClient
{
    Task LoginAsync();
    
    Task LogoutAsync();
    
    Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync();

    IAsyncDisposable AutoLogout();
    
    NetworkCredential? Credential { get; set; }
}