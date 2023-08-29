using System.Net;
using CreativeCoders.Net.JsonRpc;
using CreativeCoders.Net.JsonRpc.ApiBuilder;

namespace CreativeCoders.HomeMatic.JsonRpc;

[JsonRpcApi(IncludeParameterNames = true)]
public interface IHomeMaticJsonRpcApi
{
    [JsonRpcMethod("Session.login")]
    Task<JsonRpcResponse<string>> LoginAsync([JsonRpcArgument("username")] string userName, string password);
    
    [JsonRpcMethod("Session.login")]
    Task<string> DoLoginAsync(string username, string password);
    
    [JsonRpcMethod("Session.logout")]
    Task<JsonRpcResponse<bool>> LogoutAsync([JsonRpcArgument("_session_id_")]string sessionId);
    
    [JsonRpcMethod("Device.listAllDetail")]
    Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ListAllDetailsAsync([JsonRpcArgument("_session_id_")]string sessionId);
    
    [JsonRpcMethod("Device.listAllDetail")]
    Task<JsonRpcResponse<IEnumerable<DeviceDetails>>> ListAllDetailsAsync();
    
    string CcuHost { get; set; }
    
    ICredentials Credentials { get; set; }
}