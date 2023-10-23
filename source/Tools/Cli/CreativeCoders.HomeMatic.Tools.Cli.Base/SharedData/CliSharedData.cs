using System.Collections.Concurrent;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;

public class CliSharedData
{
    public string CcuHost { get; set; } = string.Empty;

    public IDictionary<string, string> Users { get; set; } = new ConcurrentDictionary<string, string>();
}