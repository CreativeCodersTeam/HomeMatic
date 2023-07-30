using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Client;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcApi
{
    [JsonRpcMethod("Session.login")]
    Task<JsonRpcResponse<string>> LoginAsync(string username, string password);
    
    [JsonRpcMethod("Session.logout")]
    Task<JsonRpcResponse<bool>> LogoutAsync(string sessionId);
    
    [JsonRpcMethod("Device.listAllDetail")]
    Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ListAllDetailsAsync(string sessionId);
}

public class HomeMaticJsonRpcApi : IHomeMaticJsonRpcApi
{
    private readonly HttpClient _httpClient;

    public HomeMaticJsonRpcApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = Ensure.NotNull(httpClient, nameof(httpClient));
    }
    
    public Task<JsonRpcResponse<string>> LoginAsync(string username, string password)
    {
        var loginResponse = await jsonRpcClient.ExecuteAsync<string>(
            new Uri($"http://{cliData.CcuHost}/api/homematic.cgi"),
            "Session.login",
            "username", userName,
            "password", SharedData.GetPassword(cliData.CcuHost)).ConfigureAwait(false);
    }

    public Task<JsonRpcResponse<bool>> LogoutAsync(string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ListAllDetailsAsync(string sessionId)
    {
        throw new NotImplementedException();
    }
}
