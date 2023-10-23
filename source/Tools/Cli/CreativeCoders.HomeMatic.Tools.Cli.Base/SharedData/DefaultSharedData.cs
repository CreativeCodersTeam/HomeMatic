using System.Text.Json;
using CreativeCoders.Core;
using CreativeCoders.Core.IO;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;

public class DefaultSharedData : ISharedData
{
    private readonly IAnsiConsole _console;

    public DefaultSharedData(IAnsiConsole console)
    {
        _console = Ensure.NotNull(console, nameof(console));
        
        FileSys.Directory.CreateDirectory(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            HomeMaticToolApp.ConfigFolderName));
    }
    
    private static string GetCliDataFileName() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        HomeMaticToolApp.ConfigFolderName,
        HomeMaticToolApp.CliDataFileName);
    
    public CliSharedData LoadCliData()
    {
        if (!FileSys.File.Exists(GetCliDataFileName()))
        {
            return new CliSharedData();
        }
        
        return JsonSerializer.Deserialize<CliSharedData>(FileSys.File.ReadAllText(GetCliDataFileName()))
            ?? new CliSharedData();
    }

    public void SaveCliData(CliSharedData cliSharedData)
    {
        FileSys.File.WriteAllText(GetCliDataFileName(), JsonSerializer.Serialize(cliSharedData));
    }

    public string GetPassword(string ccuHost)
    {
        return _console.Prompt<string>(new TextPrompt<string>("Password: ") { IsSecret = true });
    }
}