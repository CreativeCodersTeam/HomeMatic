using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Client;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcApi
{
    [JsonRpcMethod("Session.login")]
    Task<JsonRpcResponse<string>> LoginAsync(string userName, string password);
    
    [JsonRpcMethod("Session.logout")]
    Task<JsonRpcResponse<bool>> LogoutAsync(string sessionId);
    
    [JsonRpcMethod("Device.listAllDetail")]
    Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ListAllDetailsAsync(string sessionId);
    
    string CcuHost { get; set; }
}

public class HomeMaticJsonRpcApi : IHomeMaticJsonRpcApi
{
    private readonly IJsonRpcClient _jsonRpcClient;

    public HomeMaticJsonRpcApi(IJsonRpcClient jsonRpcClient)
    {
        _jsonRpcClient = Ensure.NotNull(jsonRpcClient, nameof(jsonRpcClient));
    }
    
    public async Task<JsonRpcResponse<string>> LoginAsync(string userName, string password)
    {
        var loginResponse = await _jsonRpcClient.ExecuteAsync<string>(
            new Uri($"http://{CcuHost}/api/homematic.cgi"),
            "Session.login",
            "username", userName,
            "password", password).ConfigureAwait(false);

        return loginResponse;
    }

    public async Task<JsonRpcResponse<bool>> LogoutAsync(string sessionId)
    {
        var logoutResponse = await _jsonRpcClient.ExecuteAsync<bool>(
            new Uri($"http://{CcuHost}/api/homematic.cgi"),
            "Session.logout",
            "_session_id_", sessionId).ConfigureAwait(false);

        return logoutResponse;
    }

    public async Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ListAllDetailsAsync(string sessionId)
    {
        var listDetailsResponse = await _jsonRpcClient.ExecuteAsync<IEnumerable<DeviceDetails>>(
            new Uri($"http://{CcuHost}/api/homematic.cgi"),
            "Device.listAllDetail",
            "_session_id_", sessionId).ConfigureAwait(false);

        return listDetailsResponse;
    }

    public string CcuHost { get; set; } = string.Empty;
}
