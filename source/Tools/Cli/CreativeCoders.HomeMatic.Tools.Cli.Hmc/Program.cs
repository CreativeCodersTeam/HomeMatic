using CreativeCoders.Cli.Hosting;
using CreativeCoders.Cli.Hosting.Help;
using CreativeCoders.Configuration;
using CreativeCoders.Core.IO;
using CreativeCoders.Core.SysEnvironment;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.Tools.Cli.Base;
using CreativeCoders.HomeMatic.XmlRpc;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCoders.HomeMatic.Tools.Cli.Hmc;

internal static class Program
{
    internal static async Task<int> Main(string[] args)
    {
        var result = await CliHostBuilder.Create()
            .ConfigureServices(ConfigureServices)
            .PrintFooterText([string.Empty])
            .UseValidation()
            .UseConfiguration(x => x.TryAddJsonFile(
                FileSys.Path.Combine(
                    Env.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    HomeMaticToolApp.ConfigFolderName,
                    HomeMaticToolApp.ConfigFileName)))
            .EnableHelp(HelpCommandKind.CommandOrArgument)
            //.ScanAssemblies(typeof(ShowConfigCommand).Assembly)
            .Build()
            .RunAsync(args)
            .ConfigureAwait(false);

        return result.ExitCode;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();

        services.AddHomeMaticCliBase();

        services.AddHomeMaticXmlRpc();

        services.AddHomeMaticJsonRpc();
    }
}
