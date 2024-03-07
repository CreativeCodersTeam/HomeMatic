using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;

namespace CreativeCoders.HomeMatic.Client.Core;

public class HomeMaticCcuConnectionInfo(string? name, Uri url)
{
    public string Name { get; } = name ?? url.ToString();

    public Uri Url { get; } = Ensure.NotNull(url);

    public HomeMaticDeviceSystem Systems { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }
}
