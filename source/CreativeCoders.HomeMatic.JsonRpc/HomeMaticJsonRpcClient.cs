using System.Net;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.Net.JsonRpc;

namespace CreativeCoders.HomeMatic.JsonRpc;

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

        _sessionId = null;
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
        return new DelegateAsyncDisposable(async () => await LogoutAsync().ConfigureAwait(false));
    }

    private async Task<T> InvokeAsync<T>(Func<string, Task<T>> executeFunc)
    {
        if (_sessionId == null)
        {
            await LoginAsync().ConfigureAwait(false);
        }
        
        if (_sessionId == null)
        {
            throw new InvalidOperationException("No session id available. Logins seems to fail somehow.");
        }
        
        try
        {
            return await executeFunc(_sessionId).ConfigureAwait(false);
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
                
            return await executeFunc(_sessionId);

        }
    }

    public NetworkCredential? Credential { get; set; }
}

public sealed class DelegateAsyncDisposable : IAsyncDisposable
{
    private readonly Func<ValueTask> _disposeFunc;

    public DelegateAsyncDisposable(Func<ValueTask> disposeFunc)
    {
        _disposeFunc = Ensure.NotNull(disposeFunc);
    }

    public ValueTask DisposeAsync()
    {
        return _disposeFunc();
    }
}

public static class Check
{
    public static T NotNull<T>(T? value, string? message = null)
        where T : class
    {
        var valueCopy = value;
        
        if (valueCopy == null)
        {
            if (message == null)
            {
                throw new InvalidOperationException();
            }
            else
            {
                throw new InvalidOperationException(message);
            }
        }

        return valueCopy;
    }
}
