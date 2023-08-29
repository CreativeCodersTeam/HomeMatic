using System.Net;
using CreativeCoders.Core;
using CreativeCoders.Net.JsonRpc;

namespace CreativeCoders.HomeMatic.JsonRpc;

public class HomeMaticJsonRpcApi : IHomeMaticJsonRpcApi
{
    private readonly IJsonRpcClient _jsonRpcClient;
    
    private string? _sessionId;

    public HomeMaticJsonRpcApi(IJsonRpcClient jsonRpcClient)
    {
        _jsonRpcClient = Ensure.NotNull(jsonRpcClient, nameof(jsonRpcClient));
    }

    private async Task<T> ExecuteAsync<T>(Func<Task<T>> executeFunc)
    {
        try
        {
            return await executeFunc();
        }
        catch (JsonRpcCallException e)
        {
            if (e.ErrorCode == 400)
            {
                var credential = Credentials.GetCredential(new Uri($"http://{CcuHost}/api/homematic.cgi"), "Basic");
                
                if (credential == null)
                {
                    throw;
                }
                
                var loginResponse = await LoginAsync(credential.UserName, credential.Password).ConfigureAwait(false);
                
                if (loginResponse.Result == null)
                {
                    throw;
                }
                
                _sessionId = loginResponse.Result;
                
                return await executeFunc();
            }

            throw;
        }
    }

    public async Task<JsonRpcResponse<string>> LoginAsync(string username, string password)
    {
        var loginResponse = await _jsonRpcClient.ExecuteAsync<string>(
            new Uri($"http://{CcuHost}/api/homematic.cgi"),
            "Session.login",
            "username", username,
            "password", password).ConfigureAwait(false);

        return loginResponse;
    }

    public Task<string> DoLoginAsync(string username, string password)
    {
        throw new NotImplementedException();
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

    public async Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ListAllDetailsAsync()
    {
        return await ExecuteAsync(async () =>
        {
            var listDetailsResponse = await ListAllDetailsAsync(_sessionId);

            return listDetailsResponse;
        });
    }

    public string CcuHost { get; set; } = string.Empty;

    public ICredentials Credentials { get; set; } = new NetworkCredential();
}
