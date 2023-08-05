using System.Collections.Concurrent;
using System.Reflection;
using Castle.DynamicProxy;
using CreativeCoders.Core.Reflection;
using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.HomeMatic.JsonRpc.RpcClient;

namespace CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;

public class JsonRpcApiInterceptor<T> : InterceptorWithPropertiesBase<T>
    where T : class
{
    private readonly IJsonRpcClient _jsonRpcClient;
    
    private readonly Uri _url;

    private readonly IDictionary<MethodInfo, JsonRpcMethodInfo> _rpcMethodInfos;
    
    private readonly bool _includeParameterNames;

    public JsonRpcApiInterceptor(IJsonRpcClient jsonRpcClient, Uri url)
    {
        _jsonRpcClient = jsonRpcClient;
        _url = url;

        _rpcMethodInfos = new ConcurrentDictionary<MethodInfo, JsonRpcMethodInfo>();
        
        _includeParameterNames = typeof(T).GetCustomAttribute<JsonRpcApiAttribute>()?.IncludeParameterNames ?? false;
    }
    
    protected override void ExecuteMethod(IInvocation invocation)
    {
        if (!_rpcMethodInfos.TryGetValue(invocation.Method, out var rpcMethodInfo))
        {
            rpcMethodInfo = CreateMethodInfo(invocation.Method);
            
            _rpcMethodInfos.Add(invocation.Method, rpcMethodInfo);
        }
        
        var arguments = CreateArguments(rpcMethodInfo, invocation.Arguments).ToArray();

        var result = this.ExecuteGenericMethod<object>(
            rpcMethodInfo.HasCompleteJsonResponse ? nameof(ExecuteWithJsonResponse) : nameof(ExecuteWithOutJsonResponse),
            new[] { new GenericArgument("TResult", rpcMethodInfo.ResultType) },
            rpcMethodInfo.RpcMethodName, arguments);

        invocation.ReturnValue = result;
    }
    
    private IEnumerable<object?> CreateArguments(JsonRpcMethodInfo methodInfo, object?[] arguments)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = arguments[i];

            if (_includeParameterNames)
            {
                yield return methodInfo.ArgumentNames[i];
            }
            
            yield return argument;
        }
    }

    public async Task<JsonRpcResponse<TResult>> ExecuteWithJsonResponse<TResult>(string methodName, object?[] arguments)
    {
        var response = await _jsonRpcClient.ExecuteAsync<TResult>(_url, methodName, arguments);

        return response;
    }
    
    public async Task<TResult?> ExecuteWithOutJsonResponse<TResult>(string methodName, object?[] arguments)
    {
        var response = await _jsonRpcClient.ExecuteAsync<TResult>(_url, methodName, arguments);
        
        response.EnsureSuccess(methodName);

        return response.Result;
    }
    
    private JsonRpcMethodInfo CreateMethodInfo(MethodInfo methodInfo)
    {
        var methodAttribute = methodInfo.GetCustomAttribute<JsonRpcMethodAttribute>();

        var argumentNames = methodInfo.GetParameters().Select(x =>
        {
            var argumentAttribute = x.GetCustomAttribute<JsonRpcArgumentAttribute>();
            
            return argumentAttribute?.Name ?? x.Name;
        }).ToArray();

        var resultType = methodInfo.ReturnType;
        
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            resultType = resultType.GetGenericArguments()[0];
        }
        else
        {
            throw new InvalidOperationException("Method must return a Task<>");
        }
        
        var hasCompleteJsonResponse = resultType.IsGenericType &&
                                      resultType.GetGenericTypeDefinition() == typeof(JsonRpcResponse<>);

        return new JsonRpcMethodInfo
        {
            ArgumentNames = argumentNames,
            RpcMethodName = methodAttribute?.MethodName ?? throw new InvalidOperationException("No JSON RPC method name given"),
            HasCompleteJsonResponse = hasCompleteJsonResponse,
            ResultType = hasCompleteJsonResponse ? resultType.GetGenericArguments()[0] : resultType
        };
    }
}