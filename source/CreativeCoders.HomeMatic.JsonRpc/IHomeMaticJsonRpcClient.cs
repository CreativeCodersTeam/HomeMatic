using System.Net;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.Net.JsonRpc;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcClient
{
    Task LoginAsync();
    
    Task LogoutAsync();
    
    Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync();
    
    NetworkCredential? Credential { get; set; }
}

public class HomeMaticJsonRpcClient : IHomeMaticJsonRpcClient
{
    private readonly IHomeMaticJsonRpcApi _jsonRpcApi;
    
    private string? _sessionId;

    public HomeMaticJsonRpcClient(IHomeMaticJsonRpcApi jsonRpcApi)
    {
        _jsonRpcApi = Ensure.NotNull(jsonRpcApi);
    }
    
    public async Task LoginAsync()
    {
        if (Credential == null)
        {
            throw new InvalidOperationException("No credentials specified");
        }
        
        var response = await _jsonRpcApi.LoginAsync(Credential.UserName, Credential.Password);

        _sessionId = response.Result;
    }

    public async Task LogoutAsync()
    {
        if (_sessionId == null)
        {
            return;
        }
        
        await _jsonRpcApi.LogoutAsync(_sessionId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync()
    {
        var jsonRpcResponse = await InvokeAsync(ExecuteAsync).ConfigureAwait(false);
        
        return jsonRpcResponse.Result ?? Array.Empty<DeviceDetails>();
        
        async Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ExecuteAsync()
        {
            return await _jsonRpcApi.ListAllDetailsAsync(_sessionId!).ConfigureAwait(false);
        }
    }
    
    private async Task<T> InvokeAsync<T>(Func<Task<T>> executeFunc)
    {
        if (_sessionId == null)
        {
            await LoginAsync().ConfigureAwait(false);
        }
        
        try
        {
            return await executeFunc();
        }
        catch (JsonRpcCallException e)
        {
            if (e.ErrorCode == 400)
            {
                await LoginAsync().ConfigureAwait(false);
                
                return await executeFunc();
            }

            throw;
        }
    }

    public NetworkCredential? Credential { get; set; }
}
