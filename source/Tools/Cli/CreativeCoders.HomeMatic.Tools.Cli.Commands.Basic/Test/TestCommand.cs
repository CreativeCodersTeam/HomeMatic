using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Spectre.Console;
using System.Text.Json;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.ApiBuilder;
using CreativeCoders.HomeMatic.JsonRpc.RpcClient;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Test;

public class TestCommand : CliBaseCommand, IHomeMaticCliCommand
{
    private readonly IJsonRpcApiBuilder<IHomeMaticJsonRpcApi> _jsonRpcApiBuilder;
    private readonly IAnsiConsole _console;
    
    private readonly IHomeMaticJsonRpcApi _homeMaticJsonRpcApi;

    public TestCommand(IAnsiConsole console, IHomeMaticXmlRpcApiBuilder apiBuilder, ISharedData sharedData,
        IHomeMaticJsonRpcApi homeMaticJsonRpcApi, IJsonRpcApiBuilder<IHomeMaticJsonRpcApi> jsonRpcApiBuilder)
        : base(apiBuilder, sharedData)
    {
        _jsonRpcApiBuilder = jsonRpcApiBuilder;
        _console = Ensure.NotNull(console, nameof(console));
        
        _homeMaticJsonRpcApi = Ensure.NotNull(homeMaticJsonRpcApi, nameof(homeMaticJsonRpcApi));
    }

    public async Task<int> ExecuteAsync()
    {
        var jsonRpcClient = new JsonRpcClient(new HttpClient());
        
        var cliData = SharedData.LoadCliData();
        
        if (!cliData.Users.TryGetValue(cliData.CcuHost, out var userName))
        {
            userName = _console.Prompt<string>(new TextPrompt<string>("User name: "));
            
            cliData.Users[cliData.CcuHost] = userName;
            
            SharedData.SaveCliData(cliData);
        }
        
        _homeMaticJsonRpcApi.CcuHost = cliData.CcuHost;
        
        var api = _jsonRpcApiBuilder.ForUrl(new Uri($"http://{cliData.CcuHost}/api/homematic.cgi")).Build();

        var response = await api.LoginAsync(userName, SharedData.GetPassword(cliData.CcuHost));
        
        _console.WriteLine($"Login with api builder Response: {JsonSerializer.Serialize(response)}");
        
        var loginResponse = await _homeMaticJsonRpcApi.LoginAsync(userName, SharedData.GetPassword(cliData.CcuHost));
        
        _console.WriteLine($"Login Response: {JsonSerializer.Serialize(loginResponse)}");
        
        if (loginResponse.Result == null)
        {
            return 1;
        }
        
        var listAllDetailsResponse = await _homeMaticJsonRpcApi.ListAllDetailsAsync(loginResponse.Result);
        
        _console.WriteLine($"All Details Response: {JsonSerializer.Serialize(listAllDetailsResponse)}");
        
        var logoutResponse = await _homeMaticJsonRpcApi.LogoutAsync(loginResponse.Result);
        
        _console.WriteLine($"Logout Response: {JsonSerializer.Serialize(logoutResponse)}");
        
        return 0;
    }
}
