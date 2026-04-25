using System.Net;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.JsonRpc;

[PublicAPI]
public interface IHomeMaticJsonRpcClient
{
    Task LoginAsync();

    Task LogoutAsync();

    Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync();

    IAsyncDisposable AutoLogout();

    NetworkCredential? Credential { get; set; }
}
