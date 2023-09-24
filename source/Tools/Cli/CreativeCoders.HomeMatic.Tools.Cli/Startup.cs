using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.Tools.Cli.Base;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.SysConsole.App;
using CreativeCoders.SysConsole.Cli.Actions;
using CreativeCoders.SysConsole.Cli.Actions.Runtime;
using CreativeCoders.SysConsole.Cli.Actions.Runtime.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli;

public class Startup : ICliStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.AddHomeMaticCliBase();

        services.AddHomeMaticXmlRpc();

        services.AddHomeMaticJsonRpc();

        services.AddSingleton(_ => AnsiConsole.Create(new AnsiConsoleSettings()));

        //services.AddSingleton<ICml, Cml>();
    }

    public void Configure(ICliActionRuntimeBuilder runtimeBuilder)
    {
        runtimeBuilder.AddController<ConnectionController>();
        runtimeBuilder.AddController<DevicesController>();
        //runtimeBuilder.AddController<BasicController>();

        runtimeBuilder.UseRouting();
    }
}