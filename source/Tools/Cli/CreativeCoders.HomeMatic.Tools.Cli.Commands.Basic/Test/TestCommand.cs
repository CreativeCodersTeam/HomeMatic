using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Client;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Spectre.Console;
using System.Text.Json;
using CreativeCoders.HomeMatic.JsonRpc;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Test;

public class TestCommand : CliBaseCommand, IHomeMaticCliCommand
{
    private readonly IAnsiConsole _console;

    public TestCommand(IAnsiConsole console, IHomeMaticXmlRpcApiBuilder apiBuilder, ISharedData sharedData)
        : base(apiBuilder, sharedData)
    {
        _console = Ensure.NotNull(console, nameof(console));
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
        
        var loginResponse = await jsonRpcClient.ExecuteAsync<string>(
            new Uri($"http://{cliData.CcuHost}/api/homematic.cgi"),
            "Session.login",
            "username", userName,
            "password", SharedData.GetPassword(cliData.CcuHost)).ConfigureAwait(false);
        
        _console.WriteLine($"Login Response: {JsonSerializer.Serialize(loginResponse)}");
        
        if (loginResponse.Result == null)
        {
            return 1;
        }
        
        var listAllDetailsResponse = await jsonRpcClient.ExecuteAsync<IEnumerable<DeviceDetails>>(
            new Uri($"http://{cliData.CcuHost}/api/homematic.cgi"),
            "Device.listAllDetail",
            "_session_id_", loginResponse.Result).ConfigureAwait(false);   
        
        _console.WriteLine($"All Details Response: {JsonSerializer.Serialize(listAllDetailsResponse)}");
        
        var logoutResponse = await jsonRpcClient.ExecuteAsync<bool>(
            new Uri($"http://{cliData.CcuHost}/api/homematic.cgi"),
            "Session.logout",
            "_session_id_", loginResponse.Result).ConfigureAwait(false);

        _console.WriteLine($"Logout Response: {JsonSerializer.Serialize(logoutResponse)}");
        
        return 0;
    }
}
