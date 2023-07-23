namespace CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;

public interface ISharedData
{
    CliSharedData LoadCliData();
    
    void SaveCliData(CliSharedData cliSharedData);
}