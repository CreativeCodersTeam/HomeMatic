using System.Net;
using CreativeCoders.Core;
using CreativeCoders.Core.Threading;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.Net.JsonRpc;

namespace CreativeCoders.HomeMatic.JsonRpc;

public class HomeMaticJsonRpcClient : IHomeMaticJsonRpcClient
{
    private readonly IHomeMaticJsonRpcApi _jsonRpcApi;
    
    private SynchronizedValue<string?> _sessionId = SynchronizedValue.Create<string?>(null);

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

        _sessionId.Value = response.Result;
    }

    public async Task LogoutAsync()
    {
        if (_sessionId.Value == null)
        {
            return;
        }
        
        await _jsonRpcApi.LogoutAsync(_sessionId.Value).ConfigureAwait(false);

        _sessionId.Value = null;
    }

    public async Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync()
    {
        var jsonRpcResponse = await InvokeAsync(
                sessionId => _jsonRpcApi.ListAllDetailsAsync(sessionId))
            .ConfigureAwait(false);

        return jsonRpcResponse.Result ?? Array.Empty<DeviceDetails>();
    }

    public IAsyncDisposable AutoLogout()
    {
        return new DelegateAsyncDisposable(async () => await LogoutAsync().ConfigureAwait(false), true);
    }

    private async Task<T> InvokeAsync<T>(Func<string, Task<T>> executeFunc)
    {
        if (_sessionId.Value == null)
        {
            await LoginAsync().ConfigureAwait(false);
        }
        
        if (_sessionId.Value == null)
        {
            throw new InvalidOperationException("No session id available. Logins seems to fail somehow.");
        }
        
        try
        {
            return await executeFunc(_sessionId.Value).ConfigureAwait(false);
        }
        catch (JsonRpcCallException e)
        {
            if (e.ErrorCode != 400)
            {
                throw;
            }
            
            await LoginAsync().ConfigureAwait(false);
            
            if (_sessionId == null)
            {
                throw new InvalidOperationException("No session id available. Logins seems to fail somehow.");
            }
                
            return await executeFunc(_sessionId.Value);

        }
    }

    public NetworkCredential? Credential { get; set; }
}
