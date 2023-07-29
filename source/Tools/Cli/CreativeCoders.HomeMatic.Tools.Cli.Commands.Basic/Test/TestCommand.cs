using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Client;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Spectre.Console;
using System.Text.Json;

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
        
        var response = await jsonRpcClient.ExecuteAsync(
            new Uri($"http://{cliData.CcuHost}/api/homematic.cgi"),
            "Session.login",
            "username", userName,
            "password", SharedData.GetPassword(cliData.CcuHost)).ConfigureAwait(false);
        
        _console.MarkupLine($"Response: {JsonSerializer.Serialize(response)}");

        return 0;
    }
}
