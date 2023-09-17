using System.Net;
using CreativeCoders.HomeMatic.JsonRpc.Models;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcClient
{
    Task LoginAsync();
    
    Task LogoutAsync();
    
    Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync();
    
    NetworkCredential? Credential { get; set; }
}