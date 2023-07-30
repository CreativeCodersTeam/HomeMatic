using System.Net.Http.Json;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.JsonRpc.Client;

public class JsonRpcClient : IJsonRpcClient
{
    private readonly HttpClient _httpClient;

    public JsonRpcClient(HttpClient httpClient)
    {
        _httpClient = Ensure.NotNull(httpClient, nameof(httpClient));
    }

    public async Task<JsonRpcResponse<T>> ExecuteAsync<T>(Uri url, string methodName, params object[] arguments)
    {
        var jsonRpcRequest = new JsonRpcRequest(methodName, arguments);

        var httpResponse = await _httpClient.PostAsJsonAsync(url, jsonRpcRequest).ConfigureAwait(false);

        httpResponse.EnsureSuccessStatusCode();

        var jsonRpcResponse = await httpResponse.Content
            .ReadFromJsonAsync<JsonRpcResponse<T>>()
            .ConfigureAwait(false);

        if (jsonRpcResponse == null)
        {
            throw new InvalidOperationException();
        }

        if (jsonRpcResponse.Id != jsonRpcRequest.Id)
        {
            throw new InvalidOperationException("Json RPC id mismatch");
        }

        if (jsonRpcResponse.Error != null)
        {
            throw new JsonRpcException(jsonRpcResponse.Error.Code, jsonRpcResponse.Error.Message);
        }

        return jsonRpcResponse;
    }
}
