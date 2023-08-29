using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Spectre.Console;
using System.Text.Json;
using CreativeCoders.HomeMatic.JsonRpc;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Test;

public class TestCommand : CliBaseCommand, IHomeMaticCliCommand
{
    private readonly IHomeMaticJsonRpcApiBuilder _jsonRpcApiBuilder;
    
    private readonly IAnsiConsole _console;

    public TestCommand(IAnsiConsole console, IHomeMaticXmlRpcApiBuilder apiBuilder, ISharedData sharedData,
        IHomeMaticJsonRpcApiBuilder jsonRpcApiBuilder)
        : base(apiBuilder, sharedData)
    {
        _jsonRpcApiBuilder = jsonRpcApiBuilder;
        _console = Ensure.NotNull(console, nameof(console));
    }

    public async Task<int> ExecuteAsync()
    {
        var cliData = SharedData.LoadCliData();
        
        if (!cliData.Users.TryGetValue(cliData.CcuHost, out var userName))
        {
            userName = _console.Prompt<string>(new TextPrompt<string>("User name: "));
            
            cliData.Users[cliData.CcuHost] = userName;
            
            SharedData.SaveCliData(cliData);
        }
        
        var api = _jsonRpcApiBuilder.ForUrl(new Uri($"http://{cliData.CcuHost}/api/homematic.cgi")).Build();
        
        var loginResponse = await api.LoginAsync(userName, SharedData.GetPassword(cliData.CcuHost));
        
        _console.WriteLine($"Login Response: {JsonSerializer.Serialize(loginResponse)}");
        
        if (loginResponse.Result == null)
        {
            return 1;
        }
        
        var listAllDetailsResponse = await api.ListAllDetailsAsync(loginResponse.Result);
        
        _console.WriteLine($"All Details Response: {JsonSerializer.Serialize(listAllDetailsResponse)}");
        
        var logoutResponse = await api.LogoutAsync(loginResponse.Result);
        
        _console.WriteLine($"Logout Response: {JsonSerializer.Serialize(logoutResponse)}");
        
        return 0;
    }
}
