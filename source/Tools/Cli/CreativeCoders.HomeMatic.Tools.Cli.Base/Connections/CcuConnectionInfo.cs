using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;

public class CcuConnectionInfo(Uri url, string? name)
{
    public string Name { get; set; } = name ?? url.Host;

    public Uri Url { get; set; } = Ensure.NotNull(url);
}
