using System.Net;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.Net.JsonRpc;

namespace CreativeCoders.HomeMatic.JsonRpc;

public interface IHomeMaticJsonRpcClient
{
    Task<bool> LoginAsync();
    
    Task<bool> LogoutAsync();
    
    Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync();
    
    ICredentials? Credentials { get; set; }
}

public class HomeMaticJsonRpcClient : IHomeMaticJsonRpcClient
{
    private readonly IHomeMaticJsonRpcApi _jsonRpcApi;

    public HomeMaticJsonRpcClient(IHomeMaticJsonRpcApi jsonRpcApi)
    {
        _jsonRpcApi = Ensure.NotNull(jsonRpcApi);
    }
    
    public async Task<bool> LoginAsync()
    {
        _jsonRpcApi.LoginAsync()
    }

    public Task<bool> LogoutAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DeviceDetails>> ListAllDetailsAsync()
    {
        throw new NotImplementedException();
    }
    
    private async Task<T> InvokeAsync<T>(Func<Task<T>> executeFunc)
    {
        try
        {
            return await executeFunc();
        }
        catch (JsonRpcCallException e)
        {
            if (e.ErrorCode == 400)
            {
                //var credential = Credentials.GetCredential(new Uri($"http://{CcuHost}/api/homematic.cgi"), "Basic");
                
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

    public ICredentials? Credentials { get; set; }
}
