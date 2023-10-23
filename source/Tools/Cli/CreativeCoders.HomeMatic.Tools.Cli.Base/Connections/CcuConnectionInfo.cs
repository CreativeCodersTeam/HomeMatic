using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CcuConnectionInfo
{
    public CcuConnectionInfo(Uri url, string? name)
    {
        Url = Ensure.NotNull(url);
        Name = name ?? url.Host;
    }
    
    public string Name { get; set; }

    public Uri Url { get; set; }
}