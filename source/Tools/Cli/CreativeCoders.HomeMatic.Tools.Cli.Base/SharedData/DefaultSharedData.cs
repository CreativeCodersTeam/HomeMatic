using System.Text.Json;
using CreativeCoders.Core.IO;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;

public class DefaultSharedData : ISharedData
{
    public DefaultSharedData()
    {
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
}