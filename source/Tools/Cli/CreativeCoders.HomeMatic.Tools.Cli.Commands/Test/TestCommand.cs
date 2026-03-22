using System.Text.Json;
using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Test;

public class TestCommand(
    IAnsiConsole console,
    IHomeMaticXmlRpcApiBuilder apiBuilder,
    ISharedData sharedData,
    IHomeMaticJsonRpcApiBuilder jsonRpcApiBuilder)
    : CliBaseCommand(apiBuilder, sharedData), ICliCommand
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    public async Task<CommandResult> ExecuteAsync()
    {
        var cliData = SharedData.LoadCliData();
        if (!cliData.Users.TryGetValue(cliData.CcuHost, out var userName))
        {
            userName = await _console.PromptAsync<string>(new TextPrompt<string>("User name: "));
            cliData.Users[cliData.CcuHost] = userName;

            SharedData.SaveCliData(cliData);
        }

        var api = jsonRpcApiBuilder.ForUrl(new Uri($"http://{cliData.CcuHost}/api/homematic.cgi")).Build();

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

        return CommandResult.Success;
    }
}
